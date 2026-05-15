class RsvpResponseModel {
  const RsvpResponseModel({
    required this.resposta,
  });

  final String resposta;

  Map<String, dynamic> toJson() {
    return {
      'resposta': resposta,
    };
  }
}
