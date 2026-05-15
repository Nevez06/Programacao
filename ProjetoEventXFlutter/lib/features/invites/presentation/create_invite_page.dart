import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:projeto_eventx_flutter/core/api/api_exception.dart';
import 'package:projeto_eventx_flutter/features/invites/data/invites_repository.dart';
import 'package:projeto_eventx_flutter/features/invites/data/models/create_invite_model.dart';
import 'package:projeto_eventx_flutter/features/invites/data/models/invite_details_model.dart';
import 'package:projeto_eventx_flutter/shared/widgets/app_text_field.dart';
import 'package:projeto_eventx_flutter/shared/widgets/primary_button.dart';
import 'package:projeto_eventx_flutter/shared/widgets/section_title.dart';

class CreateInvitePage extends StatefulWidget {
  const CreateInvitePage({super.key});

  @override
  State<CreateInvitePage> createState() => _CreateInvitePageState();
}

class _CreateInvitePageState extends State<CreateInvitePage> {
  final _formKey = GlobalKey<FormState>();
  final _eventoIdController = TextEditingController();
  final _convidadoIdController = TextEditingController();
  final _nomeController = TextEditingController();
  final _emailController = TextEditingController();
  final _templateIdController = TextEditingController();
  final _mensagemController = TextEditingController();
  bool _enviarAgora = false;
  bool _saving = false;
  String? _error;

  @override
  void dispose() {
    _eventoIdController.dispose();
    _convidadoIdController.dispose();
    _nomeController.dispose();
    _emailController.dispose();
    _templateIdController.dispose();
    _mensagemController.dispose();
    super.dispose();
  }

  int? _parseInt(String value) {
    final trimmed = value.trim();
    if (trimmed.isEmpty) {
      return null;
    }
    return int.tryParse(trimmed);
  }

  Future<void> _save() async {
    if (!_formKey.currentState!.validate()) {
      return;
    }

    final eventoId = _parseInt(_eventoIdController.text);
    final convidadoId = _parseInt(_convidadoIdController.text);
    final templateId = _parseInt(_templateIdController.text);
    if (eventoId == null || eventoId <= 0) {
      setState(() {
        _error = 'EventoId invalido.';
      });
      return;
    }

    if (_convidadoIdController.text.trim().isNotEmpty && convidadoId == null) {
      setState(() {
        _error = 'ConvidadoId deve ser numerico.';
      });
      return;
    }

    if (_templateIdController.text.trim().isNotEmpty && templateId == null) {
      setState(() {
        _error = 'TemplateId deve ser numerico.';
      });
      return;
    }

    final nome = _nomeController.text.trim();
    final email = _emailController.text.trim();
    if (convidadoId == null && (nome.isEmpty || email.isEmpty)) {
      setState(() {
        _error = 'Informe ConvidadoId ou Nome+Email para criar convite.';
      });
      return;
    }

    setState(() {
      _saving = true;
      _error = null;
    });

    try {
      final result = await context.read<InvitesRepository>().createInvite(
            CreateInviteModel(
              eventoId: eventoId,
              convidadoId: convidadoId,
              nomeConvidado: nome.isEmpty ? null : nome,
              emailConvidado: email.isEmpty ? null : email,
              templateId: templateId,
              enviarAgora: _enviarAgora,
              mensagemOpcional: _mensagemController.text.trim().isEmpty
                  ? null
                  : _mensagemController.text.trim(),
            ),
          );

      if (!mounted) {
        return;
      }
      Navigator.of(context).pop<InviteDetailsModel>(result.invite);
    } on ApiException catch (error) {
      setState(() {
        _error = error.message;
      });
    } catch (_) {
      setState(() {
        _error = 'Falha ao criar convite.';
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
      appBar: AppBar(title: const Text('Criar convite')),
      body: SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(16),
          child: Form(
            key: _formKey,
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                const SectionTitle(
                  title: 'Novo convite',
                  subtitle: 'Integracao via /api/invites',
                ),
                const SizedBox(height: 16),
                AppTextField(
                  controller: _eventoIdController,
                  keyboardType: TextInputType.number,
                  label: 'EventoId',
                  validator: (value) {
                    if ((value ?? '').trim().isEmpty) {
                      return 'Informe o EventoId.';
                    }
                    return null;
                  },
                ),
                const SizedBox(height: 12),
                AppTextField(
                  controller: _convidadoIdController,
                  keyboardType: TextInputType.number,
                  label: 'ConvidadoId (opcional)',
                ),
                const SizedBox(height: 12),
                AppTextField(
                  controller: _nomeController,
                  label: 'Nome convidado (opcional)',
                ),
                const SizedBox(height: 12),
                AppTextField(
                  controller: _emailController,
                  keyboardType: TextInputType.emailAddress,
                  label: 'Email convidado (opcional)',
                ),
                const SizedBox(height: 12),
                AppTextField(
                  controller: _templateIdController,
                  keyboardType: TextInputType.number,
                  label: 'TemplateId (opcional)',
                ),
                const SizedBox(height: 12),
                AppTextField(
                  controller: _mensagemController,
                  label: 'Mensagem opcional',
                  maxLines: 3,
                ),
                const SizedBox(height: 8),
                SwitchListTile(
                  contentPadding: EdgeInsets.zero,
                  title: const Text('Enviar agora'),
                  value: _enviarAgora,
                  onChanged: (value) {
                    setState(() {
                      _enviarAgora = value;
                    });
                  },
                ),
                if (_error != null) ...[
                  const SizedBox(height: 8),
                  Text(
                    _error!,
                    style: TextStyle(
                      color: Theme.of(context).colorScheme.error,
                    ),
                  ),
                ],
                const SizedBox(height: 16),
                PrimaryButton(
                  label: 'Criar convite',
                  icon: Icons.mail_outline,
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
