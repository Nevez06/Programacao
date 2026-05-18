import 'dart:async';

import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:projeto_eventx_flutter/core/api/api_exception.dart';
import 'package:projeto_eventx_flutter/core/realtime/realtime_connection_status.dart';
import 'package:projeto_eventx_flutter/features/auth/presentation/auth_controller.dart';
import 'package:projeto_eventx_flutter/features/chat/data/chat_realtime_service.dart';
import 'package:projeto_eventx_flutter/features/chat/data/chat_repository.dart';
import 'package:projeto_eventx_flutter/features/chat/data/models/chat_message_model.dart';
import 'package:projeto_eventx_flutter/features/chat/data/models/chat_send_message_request.dart';
import 'package:projeto_eventx_flutter/features/chat/presentation/widgets/chat_loading_view.dart';
import 'package:projeto_eventx_flutter/features/chat/presentation/widgets/chat_message_bubble.dart';
import 'package:projeto_eventx_flutter/features/chat/presentation/widgets/connection_status_chip.dart';
import 'package:projeto_eventx_flutter/shared/widgets/empty_state.dart';
import 'package:projeto_eventx_flutter/shared/widgets/error_view.dart';

class ChatConversationPage extends StatefulWidget {
  const ChatConversationPage({
    required this.eventoId,
    required this.titulo,
    super.key,
  });

  final int eventoId;
  final String titulo;

  @override
  State<ChatConversationPage> createState() => _ChatConversationPageState();
}

class _ChatConversationPageState extends State<ChatConversationPage> {
  late Future<ChatHistoryResult> _futureHistory;
  final List<ChatMessageModel> _messages = <ChatMessageModel>[];
  final _messageController = TextEditingController();
  final _scrollController = ScrollController();

  StreamSubscription<ChatMessageModel>? _messagesSub;
  StreamSubscription<RealtimeConnectionStatus>? _connectionSub;
  Timer? _heartbeatTimer;

  RealtimeConnectionStatus _connectionStatus =
      RealtimeConnectionStatus.disconnected;
  bool _loadingHistory = true;
  String? _historyNote;
  String? _loadError;

  int get _currentUserId =>
      context.read<AuthController>().session?.userId ?? -1;

  @override
  void initState() {
    super.initState();
    _futureHistory = _loadHistoryAndConnect();
  }

  @override
  void dispose() {
    _messagesSub?.cancel();
    _connectionSub?.cancel();
    _heartbeatTimer?.cancel();
    _messageController.dispose();
    _scrollController.dispose();
    unawaited(_leaveAndDisconnect());
    super.dispose();
  }

  Future<void> _leaveAndDisconnect() async {
    final realtime = context.read<ChatRealtimeService>();
    await realtime.leaveConversation(widget.eventoId);
    await realtime.disconnect();
  }

  Future<ChatHistoryResult> _loadHistoryAndConnect() async {
    try {
      final history = await context.read<ChatRepository>().getHistoryForEvent(
            eventoId: widget.eventoId,
            currentUserId: _currentUserId,
          );

      if (!mounted) {
        return history;
      }

      setState(() {
        _messages
          ..clear()
          ..addAll(history.messages);
        _historyNote = history.note;
        _loadingHistory = false;
        _loadError = null;
      });
      await _connectRealtime();
      return history;
    } catch (_) {
      if (!mounted) {
        return const ChatHistoryResult(
          messages: <ChatMessageModel>[],
          historyAvailable: false,
        );
      }

      setState(() {
        _loadingHistory = false;
        _loadError = 'Nao foi possivel carregar o chat.';
      });
      rethrow;
    }
  }

  Future<void> _connectRealtime() async {
    final realtime = context.read<ChatRealtimeService>();
    _connectionStatus = realtime.status;

    _messagesSub ??= realtime.messagesStream.listen((incoming) {
      final incomingConversation = incoming.resolvedConversationId;
      if (incomingConversation != widget.eventoId && incomingConversation > 0) {
        return;
      }

      final duplicateIndex = _messages.indexWhere((existing) {
        final sameAuthor = existing.remetenteId == incoming.remetenteId;
        final sameContent =
            existing.conteudo.trim() == incoming.conteudo.trim();
        final closeTime =
            existing.dataEnvio.difference(incoming.dataEnvio).inSeconds.abs() <=
                8;
        return sameAuthor && sameContent && closeTime;
      });

      if (!mounted) {
        return;
      }

      setState(() {
        if (duplicateIndex >= 0) {
          _messages[duplicateIndex] = incoming;
        } else {
          _messages.add(incoming);
        }
      });
      _scrollToBottom();
    });

    _connectionSub ??= realtime.connectionStream.listen((status) {
      if (!mounted) {
        return;
      }
      setState(() {
        _connectionStatus = status;
      });
    });

    await realtime.ensureConnected();
    await realtime.joinConversation(widget.eventoId);
    _heartbeatTimer?.cancel();
    _heartbeatTimer = Timer.periodic(const Duration(seconds: 20), (_) {
      unawaited(realtime.heartbeat());
    });
  }

  Future<void> _refresh() async {
    setState(() {
      _futureHistory = _loadHistoryAndConnect();
    });
    await _futureHistory;
  }

  Future<void> _sendMessage() async {
    final auth = context.read<AuthController>();
    final session = auth.session;
    if (session == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Sessao invalida. Faca login novamente.')),
      );
      return;
    }

    final text = _messageController.text.trim();
    if (text.isEmpty) {
      return;
    }
    _messageController.clear();

    final localId = 'local-${DateTime.now().microsecondsSinceEpoch}';
    final local = ChatMessageModel(
      id: localId,
      eventoId: widget.eventoId,
      conversationId: widget.eventoId,
      remetenteId: session.userId,
      conteudo: text,
      dataEnvio: DateTime.now(),
      ehDoUsuarioAtual: true,
      ehAssistente: false,
      status: ChatMessageDeliveryStatus.sending,
      autorNome: session.userName,
    );

    setState(() {
      _messages.add(local);
    });
    _scrollToBottom();

    try {
      final sent = await context.read<ChatRepository>().sendMessage(
            request: ChatSendMessageRequest(
              conversationId: widget.eventoId,
              content: text,
            ),
            currentUserId: session.userId,
          );

      if (!mounted) {
        return;
      }

      setState(() {
        final index = _messages.indexWhere((entry) => entry.id == localId);
        if (index >= 0) {
          _messages[index] = (sent ?? local).copyWith(
            status: ChatMessageDeliveryStatus.sent,
          );
        }
      });
    } on ApiException catch (_) {
      // Fallback para fluxo realtime legado quando API ainda nao estiver exposta.
      try {
        await context.read<ChatRealtimeService>().sendToAssistant(
              remetenteId: session.userId,
              conteudo: text,
              eventoId: widget.eventoId,
            );

        if (!mounted) {
          return;
        }

        setState(() {
          final index = _messages.indexWhere((entry) => entry.id == localId);
          if (index >= 0) {
            _messages[index] = _messages[index].copyWith(
              status: ChatMessageDeliveryStatus.sent,
            );
          }
        });
      } catch (_) {
        if (!mounted) {
          return;
        }
        setState(() {
          final index = _messages.indexWhere((entry) => entry.id == localId);
          if (index >= 0) {
            _messages[index] = _messages[index].copyWith(
              status: ChatMessageDeliveryStatus.failed,
            );
          }
        });
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Falha ao enviar mensagem. Tentando reconectar...'),
          ),
        );
      }
    }
  }

  void _onTypingChanged(String text) {
    if (text.trim().isEmpty) {
      return;
    }
    unawaited(context.read<ChatRealtimeService>().typing(widget.eventoId));
  }

  void _scrollToBottom() {
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (!_scrollController.hasClients) {
        return;
      }
      _scrollController.animateTo(
        _scrollController.position.maxScrollExtent + 80,
        duration: const Duration(milliseconds: 220),
        curve: Curves.easeOut,
      );
    });
  }

  Widget _buildBody() {
    if (_loadingHistory) {
      return const ChatLoadingView();
    }

    if (_loadError != null) {
      return ErrorView(
        message: _loadError!,
        onRetry: _refresh,
      );
    }

    return Column(
      children: [
        Padding(
          padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
          child: Row(
            children: [
              ConnectionStatusChip(status: _connectionStatus),
              const SizedBox(width: 8),
              Expanded(
                child: Text(
                  'Conversa #${widget.eventoId}',
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                  style: Theme.of(context).textTheme.labelMedium,
                ),
              ),
            ],
          ),
        ),
        if (_historyNote != null)
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 12),
            child: Text(
              _historyNote!,
              style: Theme.of(context).textTheme.bodySmall,
            ),
          ),
        const SizedBox(height: 6),
        Expanded(
          child: RefreshIndicator(
            onRefresh: _refresh,
            child: _messages.isEmpty
                ? ListView(
                    children: const [
                      SizedBox(height: 120),
                      EmptyState(
                        icon: Icons.forum_outlined,
                        message:
                            'Sem mensagens nesta conversa ainda. Envie a primeira mensagem.',
                      ),
                    ],
                  )
                : ListView.builder(
                    controller: _scrollController,
                    padding: const EdgeInsets.symmetric(
                      horizontal: 10,
                      vertical: 8,
                    ),
                    itemCount: _messages.length,
                    itemBuilder: (context, index) => ChatMessageBubble(
                      message: _messages[index],
                    ),
                  ),
          ),
        ),
        SafeArea(
          top: false,
          child: Padding(
            padding: EdgeInsets.only(
              left: 12,
              right: 12,
              top: 8,
              bottom: MediaQuery.of(context).viewInsets.bottom > 0 ? 8 : 4,
            ),
            child: Row(
              crossAxisAlignment: CrossAxisAlignment.end,
              children: [
                Expanded(
                  child: TextField(
                    controller: _messageController,
                    textCapitalization: TextCapitalization.sentences,
                    minLines: 1,
                    maxLines: 4,
                    onChanged: _onTypingChanged,
                    decoration: const InputDecoration(
                      hintText: 'Mensagem da conversa',
                    ),
                  ),
                ),
                const SizedBox(width: 8),
                SizedBox(
                  height: 48,
                  width: 48,
                  child: FilledButton(
                    onPressed: _sendMessage,
                    child: const Icon(Icons.send),
                  ),
                ),
              ],
            ),
          ),
        ),
      ],
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text(widget.titulo),
      ),
      body: FutureBuilder<ChatHistoryResult>(
        future: _futureHistory,
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting &&
              _loadingHistory) {
            return const ChatLoadingView();
          }
          return _buildBody();
        },
      ),
    );
  }
}
