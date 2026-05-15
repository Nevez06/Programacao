import 'dart:convert';

import 'package:shared_preferences/shared_preferences.dart';
import 'package:projeto_eventx_flutter/core/constants/storage_keys.dart';
import 'package:projeto_eventx_flutter/models/auth_session.dart';

class SessionStorage {
  SessionStorage._(this._prefs);

  final SharedPreferences _prefs;

  static Future<SessionStorage> create() async {
    final prefs = await SharedPreferences.getInstance();
    return SessionStorage._(prefs);
  }

  AuthSession? readSession() {
    final raw = _prefs.getString(StorageKeys.authSession);
    if (raw == null || raw.isEmpty) {
      return null;
    }

    final json = jsonDecode(raw) as Map<String, dynamic>;
    return AuthSession.fromJson(json);
  }

  Future<void> saveSession(AuthSession session) async {
    await _prefs.setString(
        StorageKeys.authSession, jsonEncode(session.toJson()));
  }

  Future<void> clearSession() async {
    await _prefs.remove(StorageKeys.authSession);
    await clearToken();
  }

  String? readToken() => _prefs.getString(StorageKeys.authToken);

  Future<void> saveToken(String token) async {
    await _prefs.setString(StorageKeys.authToken, token);
  }

  Future<void> clearToken() async {
    await _prefs.remove(StorageKeys.authToken);
  }
}
