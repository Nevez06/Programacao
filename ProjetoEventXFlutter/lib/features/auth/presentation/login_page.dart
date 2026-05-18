import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:projeto_eventx_flutter/core/theme/app_spacing.dart';
import 'package:projeto_eventx_flutter/features/auth/presentation/auth_controller.dart';
import 'package:projeto_eventx_flutter/routes/app_routes.dart';
import 'package:projeto_eventx_flutter/shared/layouts/auth_shell.dart';
import 'package:projeto_eventx_flutter/shared/components/components.dart';

enum LoginMode {
  organizer,
  guest,
  vendor,
}

class LoginPage extends StatefulWidget {
  const LoginPage({
    this.mode = LoginMode.organizer,
    super.key,
  });

  final LoginMode mode;

  @override
  State<LoginPage> createState() => _LoginPageState();
}

class _LoginPageState extends State<LoginPage> {
  final _formKey = GlobalKey<FormState>();
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();

  String get _title => switch (widget.mode) {
        LoginMode.organizer => 'Login Organizador',
        LoginMode.guest => 'Login Convidado',
        LoginMode.vendor => 'Login Fornecedor',
      };

  String get _subtitle => switch (widget.mode) {
        LoginMode.organizer => 'Acesse seu painel operacional completo.',
        LoginMode.guest => 'Entre no universo social de eventos.',
        LoginMode.vendor => 'Gerencie presenca e oportunidades no marketplace.',
      };

  String get _cta => switch (widget.mode) {
        LoginMode.organizer => 'Entrar no painel',
        LoginMode.guest => 'Entrar no social',
        LoginMode.vendor => 'Entrar como fornecedor',
      };

  @override
  void dispose() {
    _emailController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) {
      return;
    }

    final authController = context.read<AuthController>();
    final success = await authController.login(
      email: _emailController.text,
      password: _passwordController.text,
    );

    if (!mounted) {
      return;
    }

    if (success) {
      final target = authController.resolveHomeRoute();
      Navigator.of(context).pushNamedAndRemoveUntil(
        target,
        (route) => false,
      );
      return;
    }

    final message = authController.errorMessage ?? 'Falha ao realizar login.';
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text(message)),
    );
  }

  @override
  Widget build(BuildContext context) {
    final authController = context.watch<AuthController>();

    return AuthShell(
      title: _title,
      subtitle: _subtitle,
      child: Form(
        key: _formKey,
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.stretch,
          mainAxisSize: MainAxisSize.min,
          children: [
            PageHeader(
              title: _title,
              subtitle:
                  'Autenticacao via /api/auth/login com sessao ativa no app.',
            ),
            const SizedBox(height: AppSpacing.md),
            AppTextField(
              controller: _emailController,
              keyboardType: TextInputType.emailAddress,
              autofillHints: const [AutofillHints.email],
              textInputAction: TextInputAction.next,
              label: 'Email',
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
            const SizedBox(height: AppSpacing.sm),
            AppTextField(
              controller: _passwordController,
              obscureText: true,
              autofillHints: const [AutofillHints.password],
              textInputAction: TextInputAction.done,
              onSubmitted: (_) => _submit(),
              label: 'Senha',
              prefixIcon: Icons.lock_outline_rounded,
              validator: (value) {
                if ((value ?? '').isEmpty) {
                  return 'Informe a senha.';
                }
                return null;
              },
            ),
            const SizedBox(height: AppSpacing.md),
            PrimaryButton(
              label: _cta,
              icon: Icons.login_rounded,
              isLoading: authController.isSubmitting,
              onPressed: authController.isSubmitting ? null : _submit,
              expand: true,
            ),
            const SizedBox(height: AppSpacing.sm),
            GhostButton(
              label: 'Voltar para a entrada do EventX',
              icon: Icons.arrow_back_rounded,
              onPressed: () => Navigator.of(context).pushNamedAndRemoveUntil(
                AppRoutes.landing,
                (route) => false,
              ),
            ),
          ],
        ),
      ),
    );
  }
}
