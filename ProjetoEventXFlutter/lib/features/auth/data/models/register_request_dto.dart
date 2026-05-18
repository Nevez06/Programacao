class RegisterRequestDto {
  const RegisterRequestDto({
    required this.nomeCompleto,
    required this.email,
    required this.password,
    required this.tipoUsuario,
    this.userName,
    this.cpf,
    this.cnpj,
    this.endereco,
    this.telefone,
    this.cidade,
    this.uf,
    this.tipoServico,
  });

  final String nomeCompleto;
  final String email;
  final String password;
  final String tipoUsuario;
  final String? userName;
  final String? cpf;
  final String? cnpj;
  final String? endereco;
  final String? telefone;
  final String? cidade;
  final String? uf;
  final String? tipoServico;

  Map<String, dynamic> toJson() {
    final mappedUserType = _mapUserType(tipoUsuario);
    return {
      'fullName': nomeCompleto,
      'email': email,
      'password': password,
      'userType': mappedUserType,
    }..removeWhere((key, value) =>
        value == null || (value is String && value.trim().isEmpty));
  }

  static String _mapUserType(String value) {
    final normalized = value.trim().toLowerCase();
    switch (normalized) {
      case 'organizador':
      case 'organizer':
        return 'Organizer';
      case 'fornecedor':
      case 'supplier':
        return 'Supplier';
      case 'convidado':
      case 'guest':
      default:
        return 'Guest';
    }
  }
}
