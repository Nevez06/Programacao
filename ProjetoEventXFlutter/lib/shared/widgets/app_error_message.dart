import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/shared/widgets/error_view.dart';

class AppErrorMessage extends StatelessWidget {
  const AppErrorMessage({
    required this.message,
    this.onRetry,
    super.key,
  });

  final String message;
  final VoidCallback? onRetry;

  @override
  Widget build(BuildContext context) {
    return ErrorView(
      message: message,
      onRetry: onRetry,
    );
  }
}
