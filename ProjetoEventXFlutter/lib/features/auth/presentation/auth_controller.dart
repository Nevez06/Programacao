import 'dart:async';

import 'package:flutter/foundation.dart';
import 'package:projeto_eventx_flutter/core/api/api_client.dart';
import 'package:projeto_eventx_flutter/core/api/api_exception.dart';
import 'package:projeto_eventx_flutter/core/auth/auth_service.dart';
import 'package:projeto_eventx_flutter/features/auth/data/models/register_request_dto.dart';
import 'package:projeto_eventx_flutter/models/auth_session.dart';
import 'package:projeto_eventx_flutter/models/user_profile.dart';
import 'package:projeto_eventx_flutter/routes/app_routes.dart';

enum AuthStatus {
  loading,
  authenticated,
  unauthenticated,
}

class AuthController extends ChangeNotifier {
  AuthController({
    required AuthService authService,
    required ApiClient apiClient,
  })  : _authService = authService,
        _apiClient = apiClient {
    _unauthorizedSub = _apiClient.unauthorizedStream.listen((_) {
      unawaited(handleSessionExpired());
    });
  }

  final AuthService _authService;
  final ApiClient _apiClient;

  StreamSubscription<void>? _unauthorizedSub;
  bool _handlingSessionExpired = false;

  AuthStatus _status = AuthStatus.loading;
  AuthSession? _session;
  UserProfile? _profile;
  String? _errorMessage;
  bool _isSubmitting = false;

  AuthStatus get status => _status;
  AuthSession? get session => _session;
  UserProfile? get profile => _profile;
  String? get errorMessage => _errorMessage;
  bool get isSubmitting => _isSubmitting;

  bool get isAuthenticated => _status == AuthStatus.authenticated;

  Future<void> initialize() async {
    _status = AuthStatus.loading;
    _errorMessage = null;
    notifyListeners();

    final restoredSession = await _authService.restoreSession();
    if (restoredSession == null) {
      _session = null;
      _profile = null;
      _status = AuthStatus.unauthenticated;
      notifyListeners();
      return;
    }

    _session = restoredSession;
    _profile = _profileFromSession(restoredSession);
    _status = AuthStatus.authenticated;
    unawaited(_hydrateProfileSilently());
    notifyListeners();
  }

  Future<bool> login({
    required String email,
    required String password,
  }) async {
    _isSubmitting = true;
    _errorMessage = null;
    notifyListeners();

    try {
      final session = await _authService.login(
        email: email.trim(),
        password: password,
      );
      _session = session;
      _profile = _profileFromSession(session);
      _status = AuthStatus.authenticated;
      _errorMessage = null;
      unawaited(_hydrateProfileSilently());
      return true;
    } on ApiException catch (error) {
      _errorMessage = error.message;
      _status = AuthStatus.unauthenticated;
      return false;
    } catch (_) {
      _errorMessage = 'Erro inesperado no login.';
      _status = AuthStatus.unauthenticated;
      return false;
    } finally {
      _isSubmitting = false;
      notifyListeners();
    }
  }

  Future<bool> register(RegisterRequestDto request) async {
    _isSubmitting = true;
    _errorMessage = null;
    notifyListeners();

    try {
      final session = await _authService.register(request);
      _session = session;
      _profile = _profileFromSession(session);
      _status = AuthStatus.authenticated;
      _errorMessage = null;
      unawaited(_hydrateProfileSilently());
      return true;
    } on ApiException catch (error) {
      _errorMessage = error.message;
      _status = AuthStatus.unauthenticated;
      return false;
    } catch (_) {
      _errorMessage = 'Erro inesperado ao criar conta.';
      _status = AuthStatus.unauthenticated;
      return false;
    } finally {
      _isSubmitting = false;
      notifyListeners();
    }
  }

  Future<void> logout() async {
    await _authService.logout();
    _session = null;
    _profile = null;
    _status = AuthStatus.unauthenticated;
    notifyListeners();
  }

  Future<void> refreshProfile() async {
    if (!isAuthenticated) {
      return;
    }

    try {
      _profile = await _authService.getMe();
      _errorMessage = null;
    } on ApiException catch (error) {
      _errorMessage = error.message;
      if (error.isUnauthorized) {
        await handleSessionExpired();
      }
    } catch (_) {
      _errorMessage = 'Nao foi possivel atualizar os dados do usuario.';
    } finally {
      notifyListeners();
    }
  }

  Future<void> handleSessionExpired() async {
    if (_status == AuthStatus.unauthenticated || _handlingSessionExpired) {
      return;
    }
    _handlingSessionExpired = true;
    try {
      await _authService.logout();
      _session = null;
      _profile = null;
      _status = AuthStatus.unauthenticated;
      _errorMessage = 'Sua sessao expirou. Faca login novamente.';
      notifyListeners();
    } finally {
      _handlingSessionExpired = false;
    }
  }

  String resolveHomeRoute() {
    final tipoUsuario = _session?.tipoUsuario.isNotEmpty == true
        ? _session!.tipoUsuario
        : _profile?.tipoUsuario;
    return AppRoutes.homeForTipoUsuario(tipoUsuario);
  }

  Future<void> _hydrateProfileSilently() async {
    if (_status != AuthStatus.authenticated) {
      return;
    }
    try {
      final profile = await _authService.getMe();
      if (_status != AuthStatus.authenticated) {
        return;
      }
      _profile = profile;
      _errorMessage = null;
      notifyListeners();
    } on ApiException catch (error) {
      if (error.isUnauthorized) {
        await handleSessionExpired();
      }
    } catch (_) {
      // Mantemos a sessao atual; perfil pode ser atualizado em refresh manual.
    }
  }

  UserProfile _profileFromSession(AuthSession session) {
    return UserProfile(
      id: session.userId,
      nome: session.userName,
      email: session.email,
      fotoUrl: null,
      tipoUsuario: session.tipoUsuario,
    );
  }

  @override
  void dispose() {
    _unauthorizedSub?.cancel();
    super.dispose();
  }
}
