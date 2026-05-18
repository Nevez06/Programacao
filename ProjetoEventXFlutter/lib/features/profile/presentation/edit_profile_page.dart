import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:projeto_eventx_flutter/core/api/api_exception.dart';
import 'package:projeto_eventx_flutter/features/auth/presentation/auth_controller.dart';
import 'package:projeto_eventx_flutter/features/profile/data/models/update_user_profile_model.dart';
import 'package:projeto_eventx_flutter/features/profile/data/profile_repository.dart';
import 'package:projeto_eventx_flutter/shared/widgets/app_text_field.dart';
import 'package:projeto_eventx_flutter/shared/widgets/loading_view.dart';
import 'package:projeto_eventx_flutter/shared/widgets/primary_button.dart';
import 'package:projeto_eventx_flutter/shared/widgets/section_title.dart';

class EditProfilePage extends StatefulWidget {
  const EditProfilePage({super.key});

  @override
  State<EditProfilePage> createState() => _EditProfilePageState();
}

class _EditProfilePageState extends State<EditProfilePage> {
  final _formKey = GlobalKey<FormState>();
  final _nomeController = TextEditingController();
  final _emailController = TextEditingController();
  bool _loading = true;
  bool _saving = false;
  String? _error;

  @override
  void initState() {
    super.initState();
    _loadInitialData();
  }

  @override
  void dispose() {
    _nomeController.dispose();
    _emailController.dispose();
    super.dispose();
  }

  Future<void> _loadInitialData() async {
    setState(() {
      _loading = true;
      _error = null;
    });

    try {
      final profile = await context.read<ProfileRepository>().fetchMe();
      _nomeController.text = profile.nome;
      _emailController.text = profile.email;
    } on ApiException catch (error) {
      _error = error.message;
    } catch (_) {
      _error = 'Nao foi possivel carregar os dados do perfil.';
    } finally {
      if (mounted) {
        setState(() {
          _loading = false;
        });
      }
    }
  }

  Future<void> _save() async {
    if (!_formKey.currentState!.validate()) {
      return;
    }
    final repository = context.read<ProfileRepository>();
    final authController = context.read<AuthController>();

    setState(() {
      _saving = true;
      _error = null;
    });

    try {
      await repository.updateMe(
        UpdateUserProfileModel(
          fullName: _nomeController.text.trim(),
          email: _emailController.text.trim(),
        ),
      );
      await authController.refreshProfile();

      if (!mounted) {
        return;
      }
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Perfil atualizado com sucesso.')),
      );
      Navigator.of(context).pop();
    } on ApiException catch (error) {
      setState(() {
        _error = error.message;
      });
    } catch (_) {
      setState(() {
        _error = 'Falha ao atualizar perfil.';
      });
    } finally {
      if (mounted) {
        setState(() {
          _saving = false;
        });
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Editar perfil')),
      body: _loading
          ? const LoadingView(message: 'Carregando...')
          : SafeArea(
              child: SingleChildScrollView(
                padding: const EdgeInsets.all(16),
                child: Form(
                  key: _formKey,
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: [
                      const SectionTitle(
                        title: 'Atualizar perfil',
                        subtitle: 'Salva em /api/users/me',
                      ),
                      const SizedBox(height: 16),
                      AppTextField(
                        controller: _nomeController,
                        label: 'Nome',
                        validator: (value) {
                          if ((value ?? '').trim().isEmpty) {
                            return 'Informe o nome.';
                          }
                          return null;
                        },
                      ),
                      const SizedBox(height: 12),
                      AppTextField(
                        controller: _emailController,
                        label: 'Email',
                        keyboardType: TextInputType.emailAddress,
                        validator: (value) {
                          final text = (value ?? '').trim();
                          if (text.isEmpty) {
                            return 'Informe o email.';
                          }
                          if (!text.contains('@')) {
                            return 'Informe um email valido.';
                          }
                          return null;
                        },
                      ),
                      if (_error != null) ...[
                        const SizedBox(height: 12),
                        Text(
                          _error!,
                          style: TextStyle(
                            color: Theme.of(context).colorScheme.error,
                          ),
                        ),
                      ],
                      const SizedBox(height: 16),
                      PrimaryButton(
                        label: 'Salvar',
                        icon: Icons.save,
                        isLoading: _saving,
                        onPressed: _saving ? null : _save,
                      ),
                    ],
                  ),
                ),
              ),
            ),
    );
  }
}
