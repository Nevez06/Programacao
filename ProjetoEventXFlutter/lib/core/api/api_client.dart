import 'dart:async';

import 'package:cookie_jar/cookie_jar.dart';
import 'package:dio/dio.dart';
import 'package:dio_cookie_manager/dio_cookie_manager.dart';
import 'package:flutter/foundation.dart';
import 'package:projeto_eventx_flutter/config/app_config.dart';
import 'package:projeto_eventx_flutter/core/api/api_exception.dart';
import 'package:projeto_eventx_flutter/core/api/http_client_adapter_stub.dart'
    if (dart.library.io) 'package:projeto_eventx_flutter/core/api/http_client_adapter_io.dart'
    as http_client_adapter;
import 'package:projeto_eventx_flutter/core/storage/session_storage.dart';

class ApiClient {
  ApiClient._({
    required Dio dio,
    required SessionStorage sessionStorage,
    required CookieJar cookieJar,
  })  : _dio = dio,
        _sessionStorage = sessionStorage,
        _cookieJar = cookieJar;

  final Dio _dio;
  final SessionStorage _sessionStorage;
  final CookieJar _cookieJar;

  final _unauthorizedController = StreamController<void>.broadcast();

  Stream<void> get unauthorizedStream => _unauthorizedController.stream;

  static Future<ApiClient> create({
    required SessionStorage sessionStorage,
  }) async {
    final dio = Dio(
      BaseOptions(
        baseUrl: AppConfig.apiBaseUrl,
        connectTimeout: const Duration(seconds: 15),
        sendTimeout: const Duration(seconds: 15),
        receiveTimeout: const Duration(seconds: 30),
        contentType: Headers.jsonContentType,
        responseType: ResponseType.json,
        headers: const {
          'Accept': Headers.jsonContentType,
        },
      ),
    );

    if (!kIsWeb && AppConfig.allowBadCertificates) {
      http_client_adapter.configureBadCertificateSupport(dio);
    }

    final cookieJar = await _buildCookieJar();
    if (!kIsWeb) {
      dio.interceptors.add(CookieManager(cookieJar));
    }

    final client = ApiClient._(
      dio: dio,
      sessionStorage: sessionStorage,
      cookieJar: cookieJar,
    );

    client._configureInterceptors();
    return client;
  }

  void _configureInterceptors() {
    _dio.interceptors.add(
      InterceptorsWrapper(
        onRequest: (options, handler) {
          final token = _sessionStorage.readToken();
          if (token != null && token.isNotEmpty) {
            options.headers['Authorization'] = 'Bearer $token';
          }
          handler.next(options);
        },
        onError: (error, handler) {
          final statusCode = error.response?.statusCode;
          if (statusCode == 401) {
            _unauthorizedController.add(null);
          }
          handler.next(error);
        },
      ),
    );

    if (kDebugMode) {
      _dio.interceptors.add(
        LogInterceptor(
          requestBody: false,
          responseBody: false,
          requestHeader: false,
          responseHeader: false,
          error: true,
        ),
      );
    }
  }

  Future<Map<String, dynamic>> getJson(
    String path, {
    Map<String, dynamic>? queryParameters,
    Map<String, dynamic>? headers,
  }) async {
    try {
      final response = await _dio.get<Map<String, dynamic>>(
        path,
        queryParameters: queryParameters,
        options: headers == null ? null : Options(headers: headers),
      );
      return response.data ?? <String, dynamic>{};
    } on DioException catch (error) {
      throw ApiException.fromDioException(error);
    }
  }

  Future<List<dynamic>> getList(
    String path, {
    Map<String, dynamic>? queryParameters,
    Map<String, dynamic>? headers,
  }) async {
    try {
      final response = await _dio.get<List<dynamic>>(
        path,
        queryParameters: queryParameters,
        options: headers == null ? null : Options(headers: headers),
      );
      return response.data ?? const <dynamic>[];
    } on DioException catch (error) {
      throw ApiException.fromDioException(error);
    }
  }

  Future<Map<String, dynamic>> postJson(
    String path, {
    Map<String, dynamic>? body,
    Map<String, dynamic>? headers,
  }) async {
    try {
      final response = await _dio.post<Map<String, dynamic>>(
        path,
        data: body,
        options: headers == null ? null : Options(headers: headers),
      );
      return response.data ?? <String, dynamic>{};
    } on DioException catch (error) {
      throw ApiException.fromDioException(error);
    }
  }

  Future<Map<String, dynamic>> putJson(
    String path, {
    Map<String, dynamic>? body,
    Map<String, dynamic>? headers,
  }) async {
    try {
      final response = await _dio.put<Map<String, dynamic>>(
        path,
        data: body,
        options: headers == null ? null : Options(headers: headers),
      );
      return response.data ?? <String, dynamic>{};
    } on DioException catch (error) {
      throw ApiException.fromDioException(error);
    }
  }

  Future<Map<String, dynamic>> patchJson(
    String path, {
    Map<String, dynamic>? body,
    Map<String, dynamic>? headers,
  }) async {
    try {
      final response = await _dio.patch<Map<String, dynamic>>(
        path,
        data: body,
        options: headers == null ? null : Options(headers: headers),
      );
      return response.data ?? <String, dynamic>{};
    } on DioException catch (error) {
      throw ApiException.fromDioException(error);
    }
  }

  Future<void> putVoid(
    String path, {
    Map<String, dynamic>? body,
    Map<String, dynamic>? headers,
  }) async {
    try {
      await _dio.put<void>(
        path,
        data: body,
        options: headers == null ? null : Options(headers: headers),
      );
    } on DioException catch (error) {
      throw ApiException.fromDioException(error);
    }
  }

  Future<void> deleteVoid(
    String path, {
    Map<String, dynamic>? headers,
  }) async {
    try {
      await _dio.delete<void>(
        path,
        options: headers == null ? null : Options(headers: headers),
      );
    } on DioException catch (error) {
      throw ApiException.fromDioException(error);
    }
  }

  Future<Map<String, dynamic>> deleteJson(
    String path, {
    Map<String, dynamic>? headers,
  }) async {
    try {
      final response = await _dio.delete<Map<String, dynamic>>(
        path,
        options: headers == null ? null : Options(headers: headers),
      );
      return response.data ?? <String, dynamic>{};
    } on DioException catch (error) {
      throw ApiException.fromDioException(error);
    }
  }

  SessionStorage get sessionStorage => _sessionStorage;

  Uri get baseUri => Uri.parse(_dio.options.baseUrl);

  Future<String?> readCookieHeaderForPath(String path) async {
    final targetUri = Uri.tryParse(path)?.hasScheme == true
        ? Uri.parse(path)
        : baseUri.resolve(path);
    final cookies = await _cookieJar.loadForRequest(targetUri);
    if (cookies.isEmpty) {
      return null;
    }
    return cookies.map((cookie) => '${cookie.name}=${cookie.value}').join('; ');
  }

  static Future<CookieJar> _buildCookieJar() async {
    // Sem dependencia de filesystem para manter Web + Mobile consistentes.
    return CookieJar();
  }

  void dispose() {
    _unauthorizedController.close();
  }
}
