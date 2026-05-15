class AuthSession {
  const AuthSession({
    required this.userId,
    this.userIdRaw,
    required this.email,
    required this.userName,
    required this.tipoUsuario,
    this.token,
  });

  final int userId;
  final String? userIdRaw;
  final String email;
  final String userName;
  final String tipoUsuario;
  final String? token;

  Map<String, dynamic> toJson() {
    return {
      'userId': userId,
      'userIdRaw': userIdRaw,
      'email': email,
      'userName': userName,
      'tipoUsuario': tipoUsuario,
      'token': token,
    };
  }

  factory AuthSession.fromJson(Map<String, dynamic> json) {
    return AuthSession(
      userId: (json['userId'] as num?)?.toInt() ?? 0,
      userIdRaw: json['userIdRaw']?.toString(),
      email: (json['email'] ?? '').toString(),
      userName: (json['userName'] ?? '').toString(),
      tipoUsuario: (json['tipoUsuario'] ?? '').toString(),
      token: json['token']?.toString(),
    );
  }

  AuthSession copyWith({
    int? userId,
    String? userIdRaw,
    String? email,
    String? userName,
    String? tipoUsuario,
    String? token,
  }) {
    return AuthSession(
      userId: userId ?? this.userId,
      userIdRaw: userIdRaw ?? this.userIdRaw,
      email: email ?? this.email,
      userName: userName ?? this.userName,
      tipoUsuario: tipoUsuario ?? this.tipoUsuario,
      token: token ?? this.token,
    );
  }
}
