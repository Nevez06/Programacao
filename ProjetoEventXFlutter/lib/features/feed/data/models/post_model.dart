import 'package:projeto_eventx_flutter/core/utils/id_value_parser.dart';

class PostModel {
  const PostModel({
    required this.id,
    required this.autorUserId,
    required this.autorNome,
    this.autorFotoUrl,
    this.titulo,
    required this.conteudo,
    this.imagemUrl,
    this.categoria,
    this.tipoConteudo,
    this.localizacao,
    required this.dataCriacao,
    required this.dataAtualizacao,
    required this.totalCurtidas,
    required this.totalComentarios,
    required this.totalCompartilhamentos,
    required this.totalVisualizacoes,
    required this.usuarioCurtiu,
    required this.isOwner,
    required this.commentsEnabled,
    required this.isPinned,
    required this.isArchived,
    required this.hideLikesCount,
    required this.hideSharesCount,
    this.eventoId,
    this.nomeEvento,
    required this.comentarios,
  });

  final int id;
  final int autorUserId;
  final String autorNome;
  final String? autorFotoUrl;
  final String? titulo;
  final String conteudo;
  final String? imagemUrl;
  final String? categoria;
  final String? tipoConteudo;
  final String? localizacao;
  final DateTime dataCriacao;
  final DateTime dataAtualizacao;
  final int totalCurtidas;
  final int totalComentarios;
  final int totalCompartilhamentos;
  final int totalVisualizacoes;
  final bool usuarioCurtiu;
  final bool isOwner;
  final bool commentsEnabled;
  final bool isPinned;
  final bool isArchived;
  final bool hideLikesCount;
  final bool hideSharesCount;
  final int? eventoId;
  final String? nomeEvento;
  final List<PostCommentModel> comentarios;

  factory PostModel.fromJson(Map<String, dynamic> json) {
    final comentariosValue = json['comentarios'] ??
        json['comments'] ??
        json['Comentarios'] ??
        json['Comments'];
    final comentariosRaw = (comentariosValue is List<dynamic>)
        ? comentariosValue
        : const <dynamic>[];

    return PostModel(
      id: _readInt(json, ['id']),
      autorUserId: _readInt(json, ['autorUserId', 'authorId', 'userId']),
      autorNome: _readString(json, ['autorNome', 'authorName', 'nomeAutor']),
      autorFotoUrl: _readNullableString(
        json,
        ['autorFotoUrl', 'authorAvatar', 'authorAvatarUrl'],
      ),
      titulo: _readNullableString(json, ['titulo', 'title']),
      conteudo: _readString(json, ['conteudo', 'caption', 'content']),
      imagemUrl:
          _readNullableString(json, ['imagemUrl', 'imageUrl', 'mediaUrl']),
      categoria: _readNullableString(json, ['categoria', 'category']),
      tipoConteudo: _readNullableString(json, ['tipoConteudo', 'contentType']),
      localizacao: _readNullableString(json, ['localizacao', 'location']),
      dataCriacao:
          _readDateTime(json, ['dataCriacao', 'createdAt', 'createdAtUtc']),
      dataAtualizacao: _readDateTime(json, [
        'dataAtualizacao',
        'updatedAt',
        'updatedAtUtc',
        'createdAtUtc',
        'createdAt'
      ]),
      totalCurtidas: _readInt(json, ['totalCurtidas', 'likesCount']),
      totalComentarios: _readInt(json, ['totalComentarios', 'commentsCount']),
      totalCompartilhamentos:
          _readInt(json, ['totalCompartilhamentos', 'sharesCount']),
      totalVisualizacoes: _readInt(json, ['totalVisualizacoes', 'viewsCount']),
      usuarioCurtiu: _readBool(json, ['usuarioCurtiu', 'isLikedByCurrentUser']),
      isOwner: _readBool(json, ['isOwner']),
      commentsEnabled: _readBool(json, ['commentsEnabled'], fallback: true),
      isPinned: _readBool(json, ['isPinned']),
      isArchived: _readBool(json, ['isArchived']),
      hideLikesCount: _readBool(json, ['hideLikesCount']),
      hideSharesCount: _readBool(json, ['hideSharesCount']),
      eventoId: _readNullableInt(json, ['eventoId', 'eventId']),
      nomeEvento: _readNullableString(json, ['nomeEvento', 'eventName']),
      comentarios: comentariosRaw
          .whereType<Map<String, dynamic>>()
          .map(PostCommentModel.fromJson)
          .toList(),
    );
  }

  PostModel copyWith({
    int? totalCurtidas,
    int? totalComentarios,
    int? totalCompartilhamentos,
    int? totalVisualizacoes,
    bool? usuarioCurtiu,
    List<PostCommentModel>? comentarios,
  }) {
    return PostModel(
      id: id,
      autorUserId: autorUserId,
      autorNome: autorNome,
      autorFotoUrl: autorFotoUrl,
      titulo: titulo,
      conteudo: conteudo,
      imagemUrl: imagemUrl,
      categoria: categoria,
      tipoConteudo: tipoConteudo,
      localizacao: localizacao,
      dataCriacao: dataCriacao,
      dataAtualizacao: dataAtualizacao,
      totalCurtidas: totalCurtidas ?? this.totalCurtidas,
      totalComentarios: totalComentarios ?? this.totalComentarios,
      totalCompartilhamentos:
          totalCompartilhamentos ?? this.totalCompartilhamentos,
      totalVisualizacoes: totalVisualizacoes ?? this.totalVisualizacoes,
      usuarioCurtiu: usuarioCurtiu ?? this.usuarioCurtiu,
      isOwner: isOwner,
      commentsEnabled: commentsEnabled,
      isPinned: isPinned,
      isArchived: isArchived,
      hideLikesCount: hideLikesCount,
      hideSharesCount: hideSharesCount,
      eventoId: eventoId,
      nomeEvento: nomeEvento,
      comentarios: comentarios ?? this.comentarios,
    );
  }

  static int _readInt(Map<String, dynamic> json, List<String> keys) {
    final value = _readValue(json, keys);
    return IdValueParser.parseToInt(value);
  }

  static int? _readNullableInt(Map<String, dynamic> json, List<String> keys) {
    final value = _readValue(json, keys);
    return IdValueParser.parseToNullableInt(value);
  }

  static String _readString(Map<String, dynamic> json, List<String> keys) {
    final value = _readValue(json, keys);
    return (value ?? '').toString();
  }

  static String? _readNullableString(
    Map<String, dynamic> json,
    List<String> keys,
  ) {
    final value = _readValue(json, keys);
    final text = value?.toString();
    if (text == null || text.isEmpty) {
      return null;
    }
    return text;
  }

  static bool _readBool(
    Map<String, dynamic> json,
    List<String> keys, {
    bool fallback = false,
  }) {
    final value = _readValue(json, keys);
    if (value is bool) {
      return value;
    }
    if (value is String) {
      if (value.toLowerCase() == 'true') {
        return true;
      }
      if (value.toLowerCase() == 'false') {
        return false;
      }
    }
    return fallback;
  }

  static DateTime _readDateTime(Map<String, dynamic> json, List<String> keys) {
    final value = _readValue(json, keys);
    return DateTime.tryParse((value ?? '').toString()) ??
        DateTime.fromMillisecondsSinceEpoch(0);
  }

  static dynamic _readValue(Map<String, dynamic> json, List<String> keys) {
    for (final key in keys) {
      if (json.containsKey(key)) {
        return json[key];
      }
      final pascal = _toPascalCase(key);
      if (json.containsKey(pascal)) {
        return json[pascal];
      }
    }
    return null;
  }

  static String _toPascalCase(String value) {
    if (value.isEmpty) {
      return value;
    }
    return value[0].toUpperCase() + value.substring(1);
  }
}

class PostCommentModel {
  const PostCommentModel({
    required this.id,
    required this.postId,
    required this.userId,
    required this.nomeAutor,
    required this.texto,
    required this.criadoEm,
    required this.isOwner,
  });

  final int id;
  final int postId;
  final int userId;
  final String nomeAutor;
  final String texto;
  final DateTime criadoEm;
  final bool isOwner;

  factory PostCommentModel.fromJson(Map<String, dynamic> json) {
    return PostCommentModel(
      id: PostModel._readInt(json, ['id']),
      postId: PostModel._readInt(json, ['postId']),
      userId: PostModel._readInt(json, ['userId']),
      nomeAutor: PostModel._readString(
        json,
        ['nomeAutor', 'authorName', 'userName'],
      ),
      texto: PostModel._readString(json, ['texto', 'text', 'content']),
      criadoEm: PostModel._readDateTime(
          json, ['criadoEm', 'createdAt', 'createdAtUtc']),
      isOwner: PostModel._readBool(json, ['isOwner']),
    );
  }
}
