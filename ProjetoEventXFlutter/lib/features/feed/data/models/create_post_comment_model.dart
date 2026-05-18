class CreatePostCommentModel {
  const CreatePostCommentModel({
    required this.texto,
  });

  final String texto;

  Map<String, dynamic> toJson() {
    return {
      'content': texto,
      'texto': texto,
    };
  }
}
