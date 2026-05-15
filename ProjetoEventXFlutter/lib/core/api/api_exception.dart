import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:projeto_eventx_flutter/core/error/app_error.dart';
import 'package:projeto_eventx_flutter/core/network/network_exception.dart';

class ApiException extends NetworkException {
  const ApiException({
    required super.message,
    required super.kind,
    super.statusCode,
  });

  bool get isUnauthorized => kind == AppErrorKind.unauthorized;
  bool get isNotFound => kind == AppErrorKind.notFound;
  bool get isForbidden => kind == AppErrorKind.forbidden;

  factory ApiException.fromDioException(DioException error) {
    final statusCode = error.response?.statusCode;
    final data = error.response?.data;
    final method = error.requestOptions.method;
    final uri = error.requestOptions.uri.toString();

    var kind = AppErrorKind.unknown;
    String message = 'Falha ao comunicar com a API.';

    if (data is Map<String, dynamic>) {
      final apiMessage = data['message'] ?? data['title'] ?? data['detail'];
      if (apiMessage != null && apiMessage.toString().trim().isNotEmpty) {
        message = apiMessage.toString();
      }
    } else if (data is String && data.trim().isNotEmpty) {
      message = data;
    }

    if (error.type == DioExceptionType.connectionTimeout ||
        error.type == DioExceptionType.receiveTimeout ||
        error.type == DioExceptionType.sendTimeout) {
      kind = AppErrorKind.timeout;
      message = 'Tempo de conexao esgotado. [$method $uri]';
    } else if (error.type == DioExceptionType.connectionError) {
      kind = AppErrorKind.noInternet;
      final detail = (error.message ?? '').trim();
      message = detail.isEmpty
          ? 'Sem conexao com a internet. [$method $uri]'
          : 'Sem conexao com a internet. [$method $uri] $detail';
    } else {
      kind = switch (statusCode) {
        401 => AppErrorKind.unauthorized,
        403 => AppErrorKind.forbidden,
        404 => AppErrorKind.notFound,
        500 || 502 || 503 || 504 => AppErrorKind.server,
        _ => AppErrorKind.unknown,
      };

      if (statusCode == 401 && message == 'Falha ao comunicar com a API.') {
        message = 'Nao autorizado.';
      } else if (statusCode == 404 &&
          message == 'Falha ao comunicar com a API.') {
        message = 'Recurso nao encontrado.';
      }
    }

    debugPrint(
      '[ApiException] type=${error.type} status=$statusCode request=$method $uri message=$message raw=${error.message}',
    );
    if (data != null) {
      debugPrint('[ApiException] responseData=$data');
    }

    return ApiException(
      message: message,
      kind: kind,
      statusCode: statusCode,
    );
  }

  @override
  String toString() {
    return 'ApiException(kind: $kind, statusCode: $statusCode, message: $message)';
  }
}
