class CreatePostModel {
  const CreatePostModel({
    this.titulo,
    required this.conteudo,
    this.imagemUrl,
    this.categoria,
    this.tipoConteudo,
    this.localizacao,
    this.eventoId,
    this.commentsEnabled,
  });

  final String? titulo;
  final String conteudo;
  final String? imagemUrl;
  final String? categoria;
  final String? tipoConteudo;
  final String? localizacao;
  final int? eventoId;
  final bool? commentsEnabled;

  Map<String, dynamic> toJson() {
    return {
      'caption': conteudo,
      'imageUrl': imagemUrl,
      'titulo': titulo,
      'conteudo': conteudo,
      'imagemUrl': imagemUrl,
      'categoria': categoria,
      'tipoConteudo': tipoConteudo,
      'localizacao': localizacao,
      'eventoId': eventoId,
      'commentsEnabled': commentsEnabled,
    }..removeWhere((key, value) => value == null);
  }
}
