class UpdateSocialProfileModel {
  const UpdateSocialProfileModel({
    this.nomeExibicao,
    this.username,
    this.bio,
    this.fotoPerfilUrl,
    this.cidade,
    this.instagram,
    this.site,
  });

  final String? nomeExibicao;
  final String? username;
  final String? bio;
  final String? fotoPerfilUrl;
  final String? cidade;
  final String? instagram;
  final String? site;

  Map<String, dynamic> toJson() {
    return {
      'displayName': nomeExibicao,
      'userName': username,
      'avatarUrl': fotoPerfilUrl,
      'nomeExibicao': nomeExibicao,
      'username': username,
      'bio': bio,
      'fotoPerfilUrl': fotoPerfilUrl,
      'cidade': cidade,
      'instagram': instagram,
      'site': site,
    }..removeWhere((key, value) => value == null);
  }
}
