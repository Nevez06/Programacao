import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:projeto_eventx_flutter/features/auth/data/models/register_request_dto.dart';
import 'package:projeto_eventx_flutter/features/auth/presentation/auth_controller.dart';
import 'package:projeto_eventx_flutter/routes/app_routes.dart';
import 'package:projeto_eventx_flutter/shared/layouts/auth_shell.dart';
import 'package:projeto_eventx_flutter/shared/widgets/app_text_field.dart';
import 'package:projeto_eventx_flutter/shared/widgets/primary_button.dart';
import 'package:projeto_eventx_flutter/shared/widgets/section_title.dart';

enum RegisterUserType {
  organizer,
  guest,
  supplier,
}

class RegisterPage extends StatefulWidget {
  const RegisterPage({super.key});

  @override
  State<RegisterPage> createState() => _RegisterPageState();
}

class _RegisterPageState extends State<RegisterPage> {
  final _formKey = GlobalKey<FormState>();
  final _nameController = TextEditingController();
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();
  final _userNameController = TextEditingController();
  final _phoneController = TextEditingController();
  final _cityController = TextEditingController();
  final _ufController = TextEditingController(text: 'SP');
  final _cpfController = TextEditingController();
  final _cnpjController = TextEditingController();
  final _serviceTypeController = TextEditingController();

  RegisterUserType _selectedType = RegisterUserType.organizer;

  @override
  void dispose() {
    _nameController.dispose();
    _emailController.dispose();
    _passwordController.dispose();
    _userNameController.dispose();
    _phoneController.dispose();
    _cityController.dispose();
    _ufController.dispose();
    _cpfController.dispose();
    _cnpjController.dispose();
    _serviceTypeController.dispose();
    super.dispose();
  }

  String get _tipoUsuario => switch (_selectedType) {
        RegisterUserType.organizer => 'Organizador',
        RegisterUserType.guest => 'Convidado',
        RegisterUserType.supplier => 'Fornecedor',
      };

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) {
      return;
    }

    final authController = context.read<AuthController>();
    final request = RegisterRequestDto(
      nomeCompleto: _nameController.text.trim(),
      email: _emailController.text.trim(),
      password: _passwordController.text,
      tipoUsuario: _tipoUsuario,
      userName: _userNameController.text.trim(),
      telefone: _phoneController.text.trim(),
      cidade: _cityController.text.trim(),
      uf: _ufController.text.trim().toUpperCase(),
      cpf: _cpfController.text.trim(),
      cnpj: _selectedType == RegisterUserType.supplier
          ? _cnpjController.text.trim()
          : null,
      tipoServico: _selectedType == RegisterUserType.supplier
          ? _serviceTypeController.text.trim()
          : null,
    );

    final success = await authController.register(request);
    if (!mounted) {
      return;
    }

    if (!success) {
      final message =
          authController.errorMessage ?? 'Nao foi possivel criar a conta.';
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(message)),
      );
      return;
    }

    Navigator.of(context).pushNamedAndRemoveUntil(
      authController.resolveHomeRoute(),
      (route) => false,
    );
  }

  @override
  Widget build(BuildContext context) {
    final authController = context.watch<AuthController>();

    return AuthShell(
      title: 'Criar Conta EventX',
      subtitle:
          'Cadastro unificado com sessao persistida para Organizer e Social.',
      child: Form(
        key: _formKey,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          mainAxisSize: MainAxisSize.min,
          children: [
            const SectionTitle(
              title: 'Cadastro',
              subtitle: 'Conta real em /api/auth/register',
            ),
            const SizedBox(height: 16),
            _UserTypeSelector(
              selectedType: _selectedType,
              onChanged: (value) => setState(() => _selectedType = value),
            ),
            const SizedBox(height: 12),
            AppTextField(
              controller: _nameController,
              label: 'Nome completo',
              prefixIcon: Icons.person_outline_rounded,
              validator: (value) =>
                  (value ?? '').trim().isEmpty ? 'Informe o nome.' : null,
            ),
            const SizedBox(height: 12),
            AppTextField(
              controller: _emailController,
              label: 'Email',
              keyboardType: TextInputType.emailAddress,
              prefixIcon: Icons.alternate_email_rounded,
              validator: (value) {
                final text = value?.trim() ?? '';
                if (text.isEmpty) {
                  return 'Informe o email.';
                }
                if (!text.contains('@')) {
                  return 'Email invalido.';
                }
                return null;
              },
            ),
            const SizedBox(height: 12),
            AppTextField(
              controller: _passwordController,
              label: 'Senha',
              obscureText: true,
              prefixIcon: Icons.lock_outline_rounded,
              validator: (value) =>
                  (value ?? '').length < 8 ? 'Minimo de 8 caracteres.' : null,
            ),
            const SizedBox(height: 12),
            AppTextField(
              controller: _userNameController,
              label: 'Username (opcional)',
              prefixIcon: Icons.badge_outlined,
            ),
            const SizedBox(height: 12),
            Row(
              children: [
                Expanded(
                  child: AppTextField(
                    controller: _cityController,
                    label: 'Cidade',
                    prefixIcon: Icons.location_city_outlined,
                  ),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: AppTextField(
                    controller: _ufController,
                    label: 'UF',
                    prefixIcon: Icons.map_outlined,
                  ),
                ),
              ],
            ),
            const SizedBox(height: 12),
            AppTextField(
              controller: _phoneController,
              label: 'Telefone (opcional)',
              keyboardType: TextInputType.phone,
              prefixIcon: Icons.phone_outlined,
            ),
            const SizedBox(height: 12),
            AppTextField(
              controller: _cpfController,
              label: 'CPF (opcional)',
              keyboardType: TextInputType.number,
              prefixIcon: Icons.credit_card_outlined,
            ),
            if (_selectedType == RegisterUserType.supplier) ...[
              const SizedBox(height: 12),
              AppTextField(
                controller: _cnpjController,
                label: 'CNPJ',
                keyboardType: TextInputType.number,
                prefixIcon: Icons.business_outlined,
                validator: (value) {
                  if (_selectedType != RegisterUserType.supplier) {
                    return null;
                  }
                  return (value ?? '').trim().isEmpty
                      ? 'Informe o CNPJ para fornecedor.'
                      : null;
                },
              ),
              const SizedBox(height: 12),
              AppTextField(
                controller: _serviceTypeController,
                label: 'Tipo de servico',
                hint: 'Buffet, Decoracao, Foto e Video...',
                prefixIcon: Icons.storefront_outlined,
              ),
            ],
            const SizedBox(height: 16),
            PrimaryButton(
              label: 'Criar conta',
              icon: Icons.app_registration_rounded,
              isLoading: authController.isSubmitting,
              expand: true,
              onPressed: authController.isSubmitting ? null : _submit,
            ),
            const SizedBox(height: 10),
            TextButton(
              onPressed: () => Navigator.of(context)
                  .pushReplacementNamed(AppRoutes.loginOrganizer),
              child: const Text('Ja tem conta? Fazer login'),
            ),
          ],
        ),
      ),
    );
  }
}

class _UserTypeSelector extends StatelessWidget {
  const _UserTypeSelector({
    required this.selectedType,
    required this.onChanged,
  });

  final RegisterUserType selectedType;
  final ValueChanged<RegisterUserType> onChanged;

  @override
  Widget build(BuildContext context) {
    return SegmentedButton<RegisterUserType>(
      segments: const [
        ButtonSegment<RegisterUserType>(
          value: RegisterUserType.organizer,
          label: Text('Organizador'),
          icon: Icon(Icons.dashboard_outlined),
        ),
        ButtonSegment<RegisterUserType>(
          value: RegisterUserType.guest,
          label: Text('Convidado'),
          icon: Icon(Icons.people_outline_rounded),
        ),
        ButtonSegment<RegisterUserType>(
          value: RegisterUserType.supplier,
          label: Text('Fornecedor'),
          icon: Icon(Icons.storefront_outlined),
        ),
      ],
      selected: {selectedType},
      onSelectionChanged: (selection) {
        if (selection.isEmpty) {
          return;
        }
        onChanged(selection.first);
      },
    );
  }
}
