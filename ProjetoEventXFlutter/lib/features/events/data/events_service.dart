import 'package:dio/dio.dart';
import 'package:projeto_eventx_flutter/config/app_config.dart';
import 'package:projeto_eventx_flutter/core/api/api_exception.dart';
import 'package:projeto_eventx_flutter/core/constants/api_endpoints.dart';
import 'package:projeto_eventx_flutter/core/error/app_error.dart';
import 'package:projeto_eventx_flutter/core/storage/session_storage.dart';

class EventsService {
  EventsService({
    required SessionStorage sessionStorage,
  })  : _sessionStorage = sessionStorage,
        _dio = Dio(
          BaseOptions(
            baseUrl: AppConfig.apiBaseUrl,
            connectTimeout: const Duration(seconds: 15),
            receiveTimeout: const Duration(seconds: 30),
            sendTimeout: const Duration(seconds: 15),
            contentType: Headers.jsonContentType,
            responseType: ResponseType.json,
            headers: const {'Accept': Headers.jsonContentType},
          ),
        );

  final SessionStorage _sessionStorage;
  final Dio _dio;

  Future<List<Map<String, dynamic>>> getEvents() async {
    final token = _sessionStorage.readToken();
    if (token == null || token.isEmpty) {
      throw ApiException(
        message: 'Sessao expirada. Faca login novamente.',
        kind: AppErrorKind.unauthorized,
        statusCode: 401,
      );
    }

    try {
      final response = await _dio.get<List<dynamic>>(
        ApiEndpoints.events,
        options: Options(
          headers: {'Authorization': 'Bearer $token'},
        ),
      );

      final rawList = response.data ?? const <dynamic>[];
      return rawList
          .whereType<Map>()
          .map((item) => Map<String, dynamic>.from(item))
          .toList(growable: false);
    } on DioException catch (error) {
      throw ApiException.fromDioException(error);
    }
  }
}
