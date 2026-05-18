import 'package:projeto_eventx_flutter/core/error/app_error.dart';

class NetworkException extends AppError {
  const NetworkException({
    required super.message,
    required super.kind,
    super.statusCode,
  });

  bool get isTimeout => kind == AppErrorKind.timeout;
  bool get isNoInternet => kind == AppErrorKind.noInternet;
}
