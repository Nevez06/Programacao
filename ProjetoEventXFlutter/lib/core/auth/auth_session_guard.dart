import 'dart:async';

import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:projeto_eventx_flutter/features/auth/presentation/auth_controller.dart';
import 'package:projeto_eventx_flutter/routes/app_routes.dart';

class AuthSessionGuard extends StatefulWidget {
  const AuthSessionGuard({
    required this.navigatorKey,
    required this.child,
    super.key,
  });

  final GlobalKey<NavigatorState> navigatorKey;
  final Widget child;

  @override
  State<AuthSessionGuard> createState() => _AuthSessionGuardState();
}

class _AuthSessionGuardState extends State<AuthSessionGuard> {
  AuthController? _controller;
  bool _wasAuthenticated = false;

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    final controller = context.read<AuthController>();
    if (!identical(_controller, controller)) {
      _controller?.removeListener(_onAuthChanged);
      _controller = controller;
      _controller?.addListener(_onAuthChanged);
    }
    _wasAuthenticated = controller.isAuthenticated;
  }

  @override
  void dispose() {
    _controller?.removeListener(_onAuthChanged);
    super.dispose();
  }

  void _onAuthChanged() {
    final controller = _controller;
    if (controller == null) {
      return;
    }

    if (controller.isAuthenticated) {
      _wasAuthenticated = true;
      return;
    }

    if (!_wasAuthenticated || controller.status == AuthStatus.loading) {
      return;
    }

    _wasAuthenticated = false;

    final navigator = widget.navigatorKey.currentState;
    if (navigator == null) {
      return;
    }

    unawaited(
      navigator.pushNamedAndRemoveUntil(
        AppRoutes.loginOrganizer,
        (route) => false,
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return widget.child;
  }
}
