import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:projeto_eventx_flutter/core/api/api_exception.dart';
import 'package:projeto_eventx_flutter/core/utils/date_formatter.dart';
import 'package:projeto_eventx_flutter/features/invites/data/invites_repository.dart';
import 'package:projeto_eventx_flutter/features/invites/data/models/invite_details_model.dart';
import 'package:projeto_eventx_flutter/features/invites/data/models/rsvp_response_model.dart';
import 'package:projeto_eventx_flutter/features/invites/data/models/update_invite_model.dart';
import 'package:projeto_eventx_flutter/shared/widgets/error_view.dart';
import 'package:projeto_eventx_flutter/shared/widgets/loading_view.dart';

class InviteDetailsPage extends StatefulWidget {
  const InviteDetailsPage({
    required this.inviteId,
    super.key,
  });

  final int inviteId;

  @override
  State<InviteDetailsPage> createState() => _InviteDetailsPageState();
}

class _InviteDetailsPageState extends State<InviteDetailsPage> {
  late Future<InviteDetailsModel> _futureInvite;
  InviteDetailsModel? _invite;
  bool _updating = false;
  bool _sending = false;
  String? _selectedStatus;
  bool _checkIn = false;

  @override
  void initState() {
    super.initState();
    _futureInvite = _loadInvite();
  }

  Future<InviteDetailsModel> _loadInvite() async {
    final invite = await context.read<InvitesRepository>().getInviteDetails(
          widget.inviteId,
        );
    if (!mounted) {
      return invite;
    }
    setState(() {
      _invite = invite;
      _selectedStatus = invite.status;
      _checkIn = invite.checkInRealizado;
    });
    return invite;
  }

  Future<void> _refresh() async {
    setState(() {
      _futureInvite = _loadInvite();
    });
    await _futureInvite;
  }

  Future<void> _updateInvite() async {
    final invite = _invite;
    if (invite == null || _updating) {
      return;
    }

    setState(() {
      _updating = true;
    });

    try {
      final updated = await context.read<InvitesRepository>().updateInvite(
            invite.id,
            UpdateInviteModel(
              status: _selectedStatus,
              checkInRealizado: _checkIn,
            ),
          );
      if (!mounted) {
        return;
      }
      setState(() {
        _invite = updated;
      });
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Convite atualizado com sucesso.')),
      );
    } on ApiException catch (error) {
      if (!mounted) {
        return;
      }
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(error.message)),
      );
    } finally {
      if (mounted) {
        setState(() {
          _updating = false;
        });
      }
    }
  }

  Future<void> _sendInvite() async {
    final invite = _invite;
    if (invite == null || _sending) {
      return;
    }

    setState(() {
      _sending = true;
    });

    try {
      final result =
          await context.read<InvitesRepository>().sendInvite(invite.id);
      if (!mounted) {
        return;
      }
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(result.message)),
      );
    } on ApiException catch (error) {
      if (!mounted) {
        return;
      }
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(error.message)),
      );
    } finally {
      if (mounted) {
        setState(() {
          _sending = false;
        });
      }
    }
  }

  Future<void> _respondRsvp(String resposta) async {
    final invite = _invite;
    if (invite == null || _updating) {
      return;
    }

    setState(() {
      _updating = true;
    });

    try {
      final updated = await context.read<InvitesRepository>().respondRsvp(
            invite.id,
            RsvpResponseModel(resposta: resposta),
          );
      if (!mounted) {
        return;
      }
      setState(() {
        _invite = updated;
        _selectedStatus = updated.status;
      });
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('RSVP atualizado para "$resposta".')),
      );
    } on ApiException catch (error) {
      if (!mounted) {
        return;
      }
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(error.message)),
      );
    } finally {
      if (mounted) {
        setState(() {
          _updating = false;
        });
      }
    }
  }

  Widget _buildBody(InviteDetailsModel invite) {
    return RefreshIndicator(
      onRefresh: _refresh,
      child: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          Text(
            invite.titulo,
            style: Theme.of(context).textTheme.headlineSmall,
          ),
          const SizedBox(height: 8),
          Text('Convidado: ${invite.nomeConvidado}'),
          Text('Email: ${invite.emailConvidado}'),
          Text('Evento: ${invite.eventoNome}'),
          Text('Status: ${invite.status}'),
          Text(
              'Data do convite: ${DateFormatter.formatDate(invite.dataConvite)}'),
          if (invite.dataEvento != null)
            Text(
                'Data do evento: ${DateFormatter.formatDate(invite.dataEvento!)}'),
          if ((invite.horaInicio ?? '').isNotEmpty)
            Text('Horario: ${invite.horaInicio} - ${invite.horaFim ?? ''}'),
          if ((invite.localNome ?? '').isNotEmpty)
            Text('Local: ${invite.localNome}'),
          if ((invite.localEndereco ?? '').isNotEmpty)
            Text('Endereco: ${invite.localEndereco}'),
          if ((invite.codigoQr ?? '').isNotEmpty)
            Text('Codigo QR: ${invite.codigoQr}'),
          const SizedBox(height: 16),
          DropdownButtonFormField<String>(
            initialValue: _selectedStatus,
            decoration: const InputDecoration(labelText: 'Status'),
            items: const [
              DropdownMenuItem(value: 'Pendente', child: Text('Pendente')),
              DropdownMenuItem(value: 'Confirmado', child: Text('Confirmado')),
              DropdownMenuItem(value: 'Nao ira', child: Text('Nao ira')),
              DropdownMenuItem(value: 'Recusado', child: Text('Recusado')),
            ],
            onChanged: (value) {
              setState(() {
                _selectedStatus = value;
              });
            },
          ),
          const SizedBox(height: 8),
          SwitchListTile(
            contentPadding: EdgeInsets.zero,
            title: const Text('Check-in realizado'),
            value: _checkIn,
            onChanged: (value) {
              setState(() {
                _checkIn = value;
              });
            },
          ),
          const SizedBox(height: 8),
          FilledButton.icon(
            onPressed: _updating ? null : _updateInvite,
            icon: const Icon(Icons.save_outlined),
            label: Text(_updating ? 'Salvando...' : 'Salvar alteracoes'),
          ),
          const SizedBox(height: 12),
          Wrap(
            spacing: 8,
            runSpacing: 8,
            children: [
              OutlinedButton(
                onPressed: _updating ? null : () => _respondRsvp('Confirmado'),
                child: const Text('RSVP Confirmado'),
              ),
              OutlinedButton(
                onPressed: _updating ? null : () => _respondRsvp('NaoIra'),
                child: const Text('RSVP Nao Ira'),
              ),
              OutlinedButton(
                onPressed: _updating ? null : () => _respondRsvp('Recusado'),
                child: const Text('RSVP Recusado'),
              ),
            ],
          ),
          const SizedBox(height: 12),
          FilledButton.tonalIcon(
            onPressed: _sending ? null : _sendInvite,
            icon: const Icon(Icons.send_outlined),
            label: Text(_sending ? 'Enviando...' : 'Enviar convite'),
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Detalhes do convite')),
      body: FutureBuilder<InviteDetailsModel>(
        future: _futureInvite,
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting &&
              _invite == null) {
            return const LoadingView(message: 'Carregando convite...');
          }

          if (snapshot.hasError && _invite == null) {
            return ErrorView(
              message: 'Nao foi possivel carregar os detalhes do convite.',
              onRetry: _refresh,
            );
          }

          final invite = _invite ?? snapshot.data;
          if (invite == null) {
            return const ErrorView(
              message: 'Convite nao encontrado.',
            );
          }

          return _buildBody(invite);
        },
      ),
    );
  }
}
