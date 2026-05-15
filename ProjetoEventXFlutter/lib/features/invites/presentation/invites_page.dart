import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:projeto_eventx_flutter/features/invites/data/invites_repository.dart';
import 'package:projeto_eventx_flutter/features/invites/data/models/invite_model.dart';
import 'package:projeto_eventx_flutter/routes/app_router.dart';
import 'package:projeto_eventx_flutter/routes/app_routes.dart';
import 'package:projeto_eventx_flutter/shared/widgets/empty_state.dart';
import 'package:projeto_eventx_flutter/shared/widgets/error_view.dart';
import 'package:projeto_eventx_flutter/shared/widgets/invite_card.dart';
import 'package:projeto_eventx_flutter/shared/widgets/loading_view.dart';
import 'package:projeto_eventx_flutter/shared/widgets/section_title.dart';

class InvitesPage extends StatefulWidget {
  const InvitesPage({
    this.embedded = false,
    super.key,
  });

  final bool embedded;

  @override
  State<InvitesPage> createState() => _InvitesPageState();
}

class _InvitesPageState extends State<InvitesPage> {
  late Future<List<InviteModel>> _futureInvites;

  @override
  void initState() {
    super.initState();
    _futureInvites = _loadInvites();
  }

  Future<List<InviteModel>> _loadInvites() {
    return context.read<InvitesRepository>().getInvites();
  }

  Future<void> _refresh() async {
    setState(() {
      _futureInvites = _loadInvites();
    });
    await _futureInvites;
  }

  Future<void> _openInviteDetails(InviteModel invite) async {
    await Navigator.of(context).pushNamed(
      AppRoutes.inviteDetails,
      arguments: InviteDetailsRouteArgs(invite.id),
    );
    if (!mounted) {
      return;
    }
    await _refresh();
  }

  Future<void> _openCreateInvite() async {
    final created = await Navigator.of(context).pushNamed(
      AppRoutes.inviteCreate,
    );

    if (!mounted || created == null) {
      return;
    }
    await _refresh();
  }

  Widget _buildBody(BuildContext context) {
    return FutureBuilder<List<InviteModel>>(
      future: _futureInvites,
      builder: (context, snapshot) {
        if (snapshot.connectionState == ConnectionState.waiting) {
          return const LoadingView(message: 'Carregando convites...');
        }

        if (snapshot.hasError) {
          return ErrorView(
            message: 'Nao foi possivel carregar os convites.',
            onRetry: _refresh,
          );
        }

        final invites = snapshot.data ?? const <InviteModel>[];
        if (invites.isEmpty) {
          return RefreshIndicator(
            onRefresh: _refresh,
            child: ListView(
              padding: const EdgeInsets.only(top: 8, bottom: 16),
              children: [
                Padding(
                  padding:
                      const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                  child: Row(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      const Expanded(
                        child: SectionTitle(
                          title: 'Convites',
                          subtitle: 'Lista vinda de /api/invites',
                        ),
                      ),
                      TextButton.icon(
                        onPressed: _openCreateInvite,
                        icon: const Icon(Icons.add),
                        label: const Text('Novo'),
                      ),
                    ],
                  ),
                ),
                const SizedBox(height: 64),
                const EmptyState(
                  icon: Icons.mark_email_unread_outlined,
                  message: 'Nenhum convite encontrado.',
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
              Padding(
                padding:
                    const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                child: Row(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const Expanded(
                      child: SectionTitle(
                        title: 'Convites',
                        subtitle: 'Lista vinda de /api/invites',
                      ),
                    ),
                    TextButton.icon(
                      onPressed: _openCreateInvite,
                      icon: const Icon(Icons.add),
                      label: const Text('Novo'),
                    ),
                  ],
                ),
              ),
              ...invites.map(
                (invite) => InviteCard(
                  invite: invite,
                  onTap: () => _openInviteDetails(invite),
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
      return _buildBody(context);
    }

    return Scaffold(
      appBar: AppBar(title: const Text('Convites')),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: _openCreateInvite,
        icon: const Icon(Icons.add),
        label: const Text('Novo convite'),
      ),
      body: _buildBody(context),
    );
  }
}
