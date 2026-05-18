import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:projeto_eventx_flutter/features/chat/data/chat_repository.dart';
import 'package:projeto_eventx_flutter/features/chat/data/models/chat_conversation_model.dart';
import 'package:projeto_eventx_flutter/features/chat/presentation/widgets/chat_conversation_item.dart';
import 'package:projeto_eventx_flutter/routes/app_router.dart';
import 'package:projeto_eventx_flutter/routes/app_routes.dart';
import 'package:projeto_eventx_flutter/shared/widgets/empty_state.dart';
import 'package:projeto_eventx_flutter/shared/widgets/error_view.dart';
import 'package:projeto_eventx_flutter/shared/widgets/section_title.dart';

class ChatListPage extends StatefulWidget {
  const ChatListPage({
    this.embedded = false,
    super.key,
  });

  final bool embedded;

  @override
  State<ChatListPage> createState() => _ChatListPageState();
}

class _ChatListPageState extends State<ChatListPage> {
  late Future<List<ChatConversationModel>> _futureConversations;

  @override
  void initState() {
    super.initState();
    _futureConversations = _loadConversations();
  }

  Future<List<ChatConversationModel>> _loadConversations() {
    return context.read<ChatRepository>().getConversations();
  }

  Future<void> _refresh() async {
    setState(() {
      _futureConversations = _loadConversations();
    });
    await _futureConversations;
  }

  Future<void> _openConversation(ChatConversationModel item) async {
    await Navigator.of(context).pushNamed(
      AppRoutes.chatConversation,
      arguments: ChatConversationRouteArgs(
        eventoId: item.eventoId,
        titulo: item.titulo,
      ),
    );
  }

  Widget _buildBody() {
    return FutureBuilder<List<ChatConversationModel>>(
      future: _futureConversations,
      builder: (context, snapshot) {
        if (snapshot.connectionState == ConnectionState.waiting) {
          return const Center(child: CircularProgressIndicator());
        }

        if (snapshot.hasError) {
          return ErrorView(
            message: 'Nao foi possivel carregar conversas de chat.',
            onRetry: _refresh,
          );
        }

        final items = snapshot.data ?? const <ChatConversationModel>[];
        if (items.isEmpty) {
          return RefreshIndicator(
            onRefresh: _refresh,
            child: ListView(
              children: const [
                SizedBox(height: 120),
                EmptyState(
                  icon: Icons.chat_bubble_outline,
                  message: 'Nenhuma conversa encontrada.',
                ),
              ],
            ),
          );
        }

        return RefreshIndicator(
          onRefresh: _refresh,
          child: ListView(
            padding: const EdgeInsets.only(top: 8, bottom: 16),
            children: [
              const Padding(
                padding: EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                child: SectionTitle(
                  title: 'Chat',
                  subtitle: 'Tempo real via /chatHub',
                ),
              ),
              const Padding(
                padding: EdgeInsets.symmetric(horizontal: 16),
                child: Text(
                  'Conectado ao backend via /api/chat/* e /chatHub.',
                ),
              ),
              const SizedBox(height: 8),
              ...items.map(
                (item) => ChatConversationItem(
                  conversation: item,
                  onTap: () => _openConversation(item),
                ),
              ),
            ],
          ),
        );
      },
    );
  }

  @override
  Widget build(BuildContext context) {
    if (widget.embedded) {
      return _buildBody();
    }

    return Scaffold(
      appBar: AppBar(title: const Text('Chat')),
      body: SafeArea(child: _buildBody()),
    );
  }
}
