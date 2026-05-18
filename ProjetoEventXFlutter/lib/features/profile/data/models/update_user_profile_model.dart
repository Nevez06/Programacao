class UpdateUserProfileModel {
  const UpdateUserProfileModel({
    this.nome,
    this.fullName,
    this.email,
    this.fotoUrl,
    this.telefone,
    this.cidade,
    this.estado,
    this.endereco,
    this.cpf,
  });

  final String? nome;
  final String? fullName;
  final String? email;
  final String? fotoUrl;
  final String? telefone;
  final String? cidade;
  final String? estado;
  final String? endereco;
  final String? cpf;

  Map<String, dynamic> toJson() {
    return {
      'fullName': fullName ?? nome,
      'email': email,
      'nome': nome ?? fullName,
      'fotoUrl': fotoUrl,
      'telefone': telefone,
      'cidade': cidade,
      'estado': estado,
      'endereco': endereco,
      'cpf': cpf,
    }..removeWhere(
        (key, value) => value == null || (value is String && value.isEmpty),
      );
  }
}
