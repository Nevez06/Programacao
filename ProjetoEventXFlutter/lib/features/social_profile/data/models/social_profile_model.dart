import 'package:projeto_eventx_flutter/core/utils/id_value_parser.dart';

class SocialProfileModel {
  const SocialProfileModel({
    required this.id,
    required this.userId,
    required this.nomeExibicao,
    required this.username,
    required this.tipoPerfil,
    required this.postsCount,
    required this.followersCount,
    required this.followingCount,
    required this.isFollowing,
    required this.atualizadoEm,
    this.bio,
    this.fotoPerfilUrl,
    this.cidade,
    this.instagram,
    this.site,
  });

  final int id;
  final int userId;
  final String nomeExibicao;
  final String username;
  final String? bio;
  final String? fotoPerfilUrl;
  final String tipoPerfil;
  final String? cidade;
  final String? instagram;
  final String? site;
  final int postsCount;
  final int followersCount;
  final int followingCount;
  final bool isFollowing;
  final DateTime atualizadoEm;

  factory SocialProfileModel.fromJson(Map<String, dynamic> json) {
    dynamic read(String key) => json[key] ?? json[_toPascalCase(key)];
    final usernameRaw = _firstNonEmpty([
      read('username'),
      read('userName'),
    ]);
    final normalizedUsername = _normalizeUsername(usernameRaw);
    final tipoPerfilRaw = _firstNonEmpty([
      read('tipoPerfil'),
      read('category'),
      read('userType'),
      read('tipoUsuario'),
    ]);
    final atualizado = DateTime.tryParse(
      _firstNonEmpty([
        read('atualizadoEm'),
        read('updatedAt'),
        read('updatedAtUtc'),
        read('createdAt'),
        read('createdAtUtc'),
      ]),
    );

    return SocialProfileModel(
      id: IdValueParser.parseToInt(read('id')),
      userId: IdValueParser.parseToInt(read('userId')),
      nomeExibicao: _firstNonEmpty([
        read('nomeExibicao'),
        read('displayName'),
        read('fullName'),
        normalizedUsername,
      ]),
      username: normalizedUsername,
      bio: _nullable(read('bio')),
      fotoPerfilUrl:
          _nullable(read('fotoPerfilUrl')) ?? _nullable(read('avatarUrl')),
      tipoPerfil: tipoPerfilRaw.isEmpty ? 'Membro' : tipoPerfilRaw,
      cidade: _nullable(read('cidade')),
      instagram: _nullable(read('instagram')),
      site: _nullable(read('site')),
      postsCount: (read('postsCount') as num?)?.toInt() ?? 0,
      followersCount: (read('followersCount') as num?)?.toInt() ?? 0,
      followingCount: (read('followingCount') as num?)?.toInt() ?? 0,
      isFollowing: (read('isFollowing') as bool?) ?? false,
      atualizadoEm: atualizado ?? DateTime.now(),
    );
  }

  static String _toPascalCase(String value) {
    if (value.isEmpty) {
      return value;
    }
    return value[0].toUpperCase() + value.substring(1);
  }

  static String? _nullable(dynamic value) {
    final text = value?.toString();
    if (text == null || text.trim().isEmpty) {
      return null;
    }
    return text;
  }

  static String _firstNonEmpty(List<dynamic> values) {
    for (final value in values) {
      final text = value?.toString().trim();
      if (text != null && text.isNotEmpty) {
        return text;
      }
    }
    return '';
  }

  static String _normalizeUsername(String username) {
    if (username.isEmpty) {
      return '@eventx';
    }
    return username.startsWith('@') ? username : '@$username';
  }
}
