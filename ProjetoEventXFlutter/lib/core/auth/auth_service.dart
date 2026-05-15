import 'package:projeto_eventx_flutter/features/auth/data/auth_repository.dart';
import 'package:projeto_eventx_flutter/features/auth/data/models/register_request_dto.dart';
import 'package:projeto_eventx_flutter/models/auth_session.dart';
import 'package:projeto_eventx_flutter/models/user_profile.dart';

class AuthService {
  AuthService(this._repository);

  final AuthRepository _repository;

  Future<AuthSession> login({
    required String email,
    required String password,
  }) {
    return _repository.login(email: email, password: password);
  }

  Future<AuthSession> register(RegisterRequestDto request) {
    return _repository.register(request);
  }

  Future<UserProfile> getMe() => _repository.fetchMe();

  Future<AuthSession?> restoreSession() => _repository.restoreSession();

  AuthSession? getSavedSession() => _repository.getSavedSession();

  Future<void> logout() => _repository.clearSession();
}
