import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:projeto_eventx_flutter/features/auth/presentation/auth_controller.dart';
import 'package:projeto_eventx_flutter/routes/app_routes.dart';
import 'package:projeto_eventx_flutter/shared/widgets/app_loader.dart';

class SplashPage extends StatefulWidget {
  const SplashPage({super.key});

  @override
  State<SplashPage> createState() => _SplashPageState();
}

class _SplashPageState extends State<SplashPage> {
  bool _navigated = false;

  void _handleNavigation(AuthController authController) {
    final status = authController.status;
    if (_navigated || status == AuthStatus.loading) {
      return;
    }

    _navigated = true;
    final target = status == AuthStatus.authenticated
        ? authController.resolveHomeRoute()
        : AppRoutes.landing;

    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (!mounted) {
        return;
      }
      Navigator.of(context).pushReplacementNamed(target);
    });
  }

  @override
  Widget build(BuildContext context) {
    final authController = context.watch<AuthController>();
    _handleNavigation(authController);

    return const Scaffold(
      body: AppLoader(message: 'Carregando EventX...'),
    );
  }
}
