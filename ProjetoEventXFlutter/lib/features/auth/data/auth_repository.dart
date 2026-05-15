import 'package:projeto_eventx_flutter/core/api/api_client.dart';
import 'package:projeto_eventx_flutter/core/api/api_exception.dart';
import 'package:projeto_eventx_flutter/core/auth/session_service.dart';
import 'package:projeto_eventx_flutter/core/constants/api_endpoints.dart';
import 'package:projeto_eventx_flutter/features/auth/data/models/login_request_dto.dart';
import 'package:projeto_eventx_flutter/features/auth/data/models/login_response_dto.dart';
import 'package:projeto_eventx_flutter/features/auth/data/models/register_request_dto.dart';
import 'package:projeto_eventx_flutter/models/auth_session.dart';
import 'package:projeto_eventx_flutter/models/user_profile.dart';

class AuthRepository {
  AuthRepository({
    required ApiClient apiClient,
    required SessionService sessionService,
  })  : _apiClient = apiClient,
        _sessionService = sessionService;

  final ApiClient _apiClient;
  final SessionService _sessionService;

  Future<AuthSession> login({
    required String email,
    required String password,
  }) async {
    final request = LoginRequestDto(
      email: email,
      password: password,
    );

    final json = await _apiClient.postJson(
      ApiEndpoints.login,
      body: request.toJson(),
    );

    final response = LoginResponseDto.fromJson(json);
    final session = response.toSession();
    await _sessionService.saveSession(session);
    if ((session.token ?? '').isNotEmpty) {
      await _sessionService.saveToken(session.token!);
    }
    return session;
  }

  Future<UserProfile> fetchMe() async {
    final json = await _apiClient.getJson(ApiEndpoints.authMe);
    return UserProfile.fromJson(json);
  }

  Future<AuthSession> register(RegisterRequestDto request) async {
    final json = await _apiClient.postJson(
      ApiEndpoints.register,
      body: request.toJson(),
    );

    final response = LoginResponseDto.fromJson(json);
    final session = response.toSession();
    await _sessionService.saveSession(session);
    if ((session.token ?? '').isNotEmpty) {
      await _sessionService.saveToken(session.token!);
    }
    return session;
  }

  AuthSession? getSavedSession() {
    return _sessionService.readSession();
  }

  Future<AuthSession?> restoreSession() async {
    try {
      return await _sessionService.restoreSession(fetchCurrentUser: fetchMe);
    } on ApiException catch (_) {
      await clearSession();
      return null;
    }
  }

  Future<void> clearSession() async {
    await _sessionService.clear();
  }
}
