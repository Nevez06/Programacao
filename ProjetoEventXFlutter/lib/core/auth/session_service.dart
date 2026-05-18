import 'package:projeto_eventx_flutter/core/storage/session_storage.dart';
import 'package:projeto_eventx_flutter/models/auth_session.dart';
import 'package:projeto_eventx_flutter/models/user_profile.dart';
import 'package:projeto_eventx_flutter/core/auth/token_storage_service.dart';

class SessionService {
  SessionService(this._sessionStorage, this._tokenStorageService);

  final SessionStorage _sessionStorage;
  final TokenStorageService _tokenStorageService;

  AuthSession? readSession() => _sessionStorage.readSession();

  String? readToken() => _tokenStorageService.readToken();

  Future<void> saveSession(AuthSession session) =>
      _sessionStorage.saveSession(session);

  Future<void> saveToken(String token) => _tokenStorageService.saveToken(token);

  Future<void> clear() async {
    await _sessionStorage.clearSession();
    await _tokenStorageService.clearToken();
  }

  Future<AuthSession?> restoreSession({
    required Future<UserProfile> Function() fetchCurrentUser,
  }) async {
    final token = readToken();
    if (token == null || token.isEmpty) {
      await clear();
      return null;
    }

    final saved = readSession();
    try {
      final me = await fetchCurrentUser();
      final restored = (saved ??
              const AuthSession(
                userId: 0,
                email: '',
                userName: '',
                tipoUsuario: '',
              ))
          .copyWith(
        userId: me.id,
        email: me.email,
        userName: me.nome,
        tipoUsuario: me.tipoUsuario,
        token: token,
      );

      await saveSession(restored);
      return restored;
    } catch (_) {
      await clear();
      return null;
    }
  }
}
