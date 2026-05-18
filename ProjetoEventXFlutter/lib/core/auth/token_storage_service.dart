import 'package:projeto_eventx_flutter/core/storage/session_storage.dart';

class TokenStorageService {
  TokenStorageService(this._sessionStorage);

  final SessionStorage _sessionStorage;

  String? readToken() => _sessionStorage.readToken();

  Future<void> saveToken(String token) => _sessionStorage.saveToken(token);

  Future<void> clearToken() => _sessionStorage.clearToken();
}
