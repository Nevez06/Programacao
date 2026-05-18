import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:projeto_eventx_flutter/features/profile/data/models/user_profile_model.dart';
import 'package:projeto_eventx_flutter/features/profile/data/profile_repository.dart';
import 'package:projeto_eventx_flutter/routes/app_routes.dart';
import 'package:projeto_eventx_flutter/shared/widgets/empty_state.dart';
import 'package:projeto_eventx_flutter/shared/widgets/error_view.dart';
import 'package:projeto_eventx_flutter/shared/widgets/loading_view.dart';
import 'package:projeto_eventx_flutter/shared/widgets/primary_button.dart';
import 'package:projeto_eventx_flutter/shared/widgets/section_title.dart';

class ProfilePage extends StatefulWidget {
  const ProfilePage({
    this.embedded = false,
    super.key,
  });

  final bool embedded;

  @override
  State<ProfilePage> createState() => _ProfilePageState();
}

class _ProfilePageState extends State<ProfilePage> {
  late Future<UserProfileModel> _futureProfile;

  @override
  void initState() {
    super.initState();
    _futureProfile = _loadProfile();
  }

  Future<UserProfileModel> _loadProfile() {
    return context.read<ProfileRepository>().fetchMe();
  }

  Future<void> _refresh() async {
    setState(() {
      _futureProfile = _loadProfile();
    });
    await _futureProfile;
  }

  Widget _buildBody(BuildContext context) {
    return FutureBuilder<UserProfileModel>(
      future: _futureProfile,
      builder: (context, snapshot) {
        if (snapshot.connectionState == ConnectionState.waiting) {
          return const LoadingView(message: 'Carregando perfil...');
        }

        if (snapshot.hasError) {
          return ErrorView(
            message: 'Nao foi possivel carregar o perfil.',
            onRetry: _refresh,
          );
        }

        final profile = snapshot.data;
        if (profile == null) {
          return const EmptyState(
            message: 'Perfil nao encontrado.',
            icon: Icons.person_off_outlined,
          );
        }

        return RefreshIndicator(
          onRefresh: _refresh,
          child: ListView(
            padding: const EdgeInsets.all(16),
            children: [
              const SectionTitle(
                title: 'Meu perfil',
                subtitle: 'Dados vindos da API /api/users/me',
              ),
              const SizedBox(height: 16),
              Center(
                child: CircleAvatar(
                  radius: 44,
                  backgroundImage: (profile.fotoUrl ?? '').isNotEmpty
                      ? NetworkImage(profile.fotoUrl!)
                      : null,
                  child: (profile.fotoUrl ?? '').isEmpty
                      ? const Icon(Icons.person, size: 44)
                      : null,
                ),
              ),
              const SizedBox(height: 16),
              Card(
                child: Padding(
                  padding: const EdgeInsets.all(12),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      _ProfileLine(label: 'Nome', value: profile.nome),
                      _ProfileLine(label: 'Email', value: profile.email),
                      _ProfileLine(
                        label: 'Tipo de usuario',
                        value: profile.tipoUsuario,
                      ),
                      _ProfileLine(
                        label: 'Status da conta',
                        value: profile.isActive ? 'Ativa' : 'Inativa',
                      ),
                      if (profile.createdAt != null)
                        _ProfileLine(
                          label: 'Conta criada em',
                          value: _formatDate(profile.createdAt!),
                        ),
                    ],
                  ),
                ),
              ),
              const SizedBox(height: 16),
              PrimaryButton(
                label: 'Editar perfil',
                icon: Icons.edit,
                onPressed: () async {
                  await Navigator.of(context).pushNamed(
                    AppRoutes.profileEdit,
                  );
                  if (!mounted) {
                    return;
                  }
                  _refresh();
                },
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
      appBar: AppBar(title: const Text('Perfil')),
      body: _buildBody(context),
    );
  }
}

class _ProfileLine extends StatelessWidget {
  const _ProfileLine({
    required this.label,
    required this.value,
  });

  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 4),
      child: Row(
        children: [
          SizedBox(
            width: 120,
            child: Text(
              '$label:',
              style: Theme.of(context).textTheme.titleSmall,
            ),
          ),
          Expanded(child: Text(value)),
        ],
      ),
    );
  }
}

String _formatDate(DateTime date) {
  final day = date.day.toString().padLeft(2, '0');
  final month = date.month.toString().padLeft(2, '0');
  return '$day/$month/${date.year}';
}
