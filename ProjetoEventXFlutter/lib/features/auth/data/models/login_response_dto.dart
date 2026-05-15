import 'package:projeto_eventx_flutter/models/auth_session.dart';

class LoginResponseDto {
  const LoginResponseDto({
    required this.userId,
    required this.userIdRaw,
    required this.email,
    required this.userName,
    required this.tipoUsuario,
    this.token,
  });

  final int userId;
  final String userIdRaw;
  final String email;
  final String userName;
  final String tipoUsuario;
  final String? token;

  factory LoginResponseDto.fromJson(Map<String, dynamic> json) {
    final user = (json['user'] ?? json['User']) as Map<String, dynamic>?;
    final source = user ?? json;
    dynamic read(String key) => source[key] ?? source[_toPascalCase(key)];
    final idRaw = (read('id') ?? read('userId') ?? '').toString();
    final parsedId = int.tryParse(idRaw) ?? 0;
    final typeValue =
        (read('userType') ?? read('tipoUsuario') ?? '').toString();
    final normalizedType = _mapUserType(typeValue);
    final fullName = (read('fullName') ?? read('userName') ?? '').toString();

    return LoginResponseDto(
      userId: parsedId,
      userIdRaw: idRaw,
      email: (read('email') ?? '').toString(),
      userName: fullName,
      tipoUsuario: normalizedType,
      token:
          (json['accessToken'] ?? json['token'] ?? read('token'))?.toString(),
    );
  }

  AuthSession toSession() {
    return AuthSession(
      userId: userId,
      userIdRaw: userIdRaw,
      email: email,
      userName: userName,
      tipoUsuario: tipoUsuario,
      token: token,
    );
  }

  static String _toPascalCase(String value) {
    if (value.isEmpty) {
      return value;
    }
    return value[0].toUpperCase() + value.substring(1);
  }

  static String _mapUserType(String value) {
    final normalized = value.trim().toLowerCase();
    switch (normalized) {
      case 'organizer':
      case 'organizador':
        return 'Organizer';
      case 'supplier':
      case 'fornecedor':
        return 'Supplier';
      case 'guest':
      case 'convidado':
      default:
        return 'Guest';
    }
  }
}
