import 'package:projeto_eventx_flutter/core/error/app_exception.dart';

enum AppErrorKind {
  unauthorized,
  forbidden,
  notFound,
  timeout,
  noInternet,
  server,
  invalidResponse,
  unknown,
}

class AppError extends AppException {
  const AppError({
    required String message,
    required this.kind,
    this.statusCode,
  }) : super(message);

  final AppErrorKind kind;
  final int? statusCode;

  bool get isAuthError => kind == AppErrorKind.unauthorized;
}
