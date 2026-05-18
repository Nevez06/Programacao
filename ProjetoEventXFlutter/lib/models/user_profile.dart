class UserProfile {
  const UserProfile({
    required this.id,
    required this.nome,
    required this.email,
    required this.fotoUrl,
    required this.tipoUsuario,
  });

  final int id;
  final String nome;
  final String email;
  final String? fotoUrl;
  final String tipoUsuario;

  factory UserProfile.fromJson(Map<String, dynamic> json) {
    final idValue = json['id'] ?? json['Id'];
    final id = idValue is num
        ? idValue.toInt()
        : int.tryParse(idValue?.toString() ?? '') ?? 0;
    final tipoUsuario =
        (json['tipoUsuario'] ?? json['userType'] ?? json['UserType'] ?? '')
            .toString();

    return UserProfile(
      id: id,
      nome: (json['nome'] ?? json['fullName'] ?? json['FullName'] ?? '')
          .toString(),
      email: (json['email'] ?? '').toString(),
      fotoUrl: (json['fotoUrl'] ?? json['photoUrl'] ?? json['avatarUrl'])
          ?.toString(),
      tipoUsuario: tipoUsuario,
    );
  }
}
