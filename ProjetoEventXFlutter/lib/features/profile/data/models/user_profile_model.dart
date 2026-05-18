import 'package:projeto_eventx_flutter/core/utils/id_value_parser.dart';

class UserProfileModel {
  const UserProfileModel({
    required this.id,
    required this.nome,
    required this.email,
    required this.fotoUrl,
    required this.tipoUsuario,
    required this.telefone,
    required this.cidade,
    required this.estado,
    required this.endereco,
    required this.cpf,
    required this.createdAt,
    required this.isActive,
  });

  final int id;
  final String nome;
  final String email;
  final String? fotoUrl;
  final String tipoUsuario;
  final String telefone;
  final String cidade;
  final String estado;
  final String endereco;
  final String cpf;
  final DateTime? createdAt;
  final bool isActive;

  factory UserProfileModel.fromJson(Map<String, dynamic> json) {
    dynamic read(String key) => json[key] ?? json[_toPascalCase(key)];
    final createdAtRaw = (read('createdAt') ?? '').toString();

    return UserProfileModel(
      id: IdValueParser.parseToInt(read('id')),
      nome: (read('nome') ?? read('name') ?? read('fullName') ?? '').toString(),
      email: (read('email') ?? '').toString(),
      fotoUrl: (read('fotoUrl') ?? read('photoUrl') ?? read('avatarUrl'))
          ?.toString(),
      tipoUsuario: (read('tipoUsuario') ?? read('userType') ?? '').toString(),
      telefone: (read('telefone') ?? '').toString(),
      cidade: (read('cidade') ?? '').toString(),
      estado: (read('estado') ?? '').toString(),
      endereco: (read('endereco') ?? '').toString(),
      cpf: (read('cpf') ?? '').toString(),
      createdAt: createdAtRaw.isEmpty ? null : DateTime.tryParse(createdAtRaw),
      isActive: (read('isActive') as bool?) ?? true,
    );
  }

  static String _toPascalCase(String value) {
    if (value.isEmpty) {
      return value;
    }
    return value[0].toUpperCase() + value.substring(1);
  }
}
