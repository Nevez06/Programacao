class ToggleLikeModel {
  const ToggleLikeModel({
    this.curtir,
  });

  final bool? curtir;

  Map<String, dynamic> toJson() {
    return {
      'curtir': curtir,
    }..removeWhere((key, value) => value == null);
  }
}

class ToggleLikeResultModel {
  const ToggleLikeResultModel({
    required this.usuarioCurtiu,
    required this.totalCurtidas,
  });

  final bool usuarioCurtiu;
  final int totalCurtidas;

  factory ToggleLikeResultModel.fromJson(Map<String, dynamic> json) {
    dynamic read(String key) => json[key] ?? json[_toPascalCase(key)];

    return ToggleLikeResultModel(
      usuarioCurtiu: (read('usuarioCurtiu') as bool?) ??
          (read('isLiked') as bool?) ??
          false,
      totalCurtidas: (read('totalCurtidas') as num?)?.toInt() ??
          (read('likesCount') as num?)?.toInt() ??
          0,
    );
  }

  static String _toPascalCase(String value) {
    if (value.isEmpty) {
      return value;
    }
    return value[0].toUpperCase() + value.substring(1);
  }
}
