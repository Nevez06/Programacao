class SaveEventModel {
  const SaveEventModel({
    required this.nomeEvento,
    required this.dataEvento,
    required this.descricaoEvento,
    required this.tipoEvento,
    required this.horaInicio,
    required this.horaFim,
    required this.publicoEstimado,
    required this.custoEstimado,
    this.localNome,
    this.statusEvento,
    this.localId,
    this.imagemCapa,
    this.permitirComentarios = true,
    this.permitirEnvioFotos = true,
    this.permitirCurtidas = true,
    this.aprovarFotosAntesPublicar = false,
    this.permitirVisualizacaoMural = true,
  });

  final String nomeEvento;
  final DateTime dataEvento;
  final String descricaoEvento;
  final String tipoEvento;
  final String? statusEvento;
  final String horaInicio;
  final String horaFim;
  final int publicoEstimado;
  final double custoEstimado;
  final String? localNome;
  final int? localId;
  final String? imagemCapa;
  final bool permitirComentarios;
  final bool permitirEnvioFotos;
  final bool permitirCurtidas;
  final bool aprovarFotosAntesPublicar;
  final bool permitirVisualizacaoMural;

  Map<String, dynamic> toJson() {
    final startDate = _combineDateAndTime(dataEvento, horaInicio);
    final endDate = _combineDateAndTime(dataEvento, horaFim);

    return {
      'name': nomeEvento,
      'type': tipoEvento,
      'description': descricaoEvento,
      'startDate': startDate.toIso8601String(),
      'endDate': endDate.toIso8601String(),
      'location': localNome,
      'coverImageUrl': imagemCapa,
      'estimatedAudience': publicoEstimado,
      'estimatedCost': custoEstimado,
      'status': _toApiStatusCode(statusEvento),
      'nomeEvento': nomeEvento,
      'dataEvento': dataEvento.toIso8601String(),
      'descricaoEvento': descricaoEvento,
      'tipoEvento': tipoEvento,
      'statusEvento': statusEvento,
      'horaInicio': horaInicio,
      'horaFim': horaFim,
      'publicoEstimado': publicoEstimado,
      'custoEstimado': custoEstimado,
      'localId': localId,
      'imagemCapa': imagemCapa,
      'permitirComentarios': permitirComentarios,
      'permitirEnvioFotos': permitirEnvioFotos,
      'permitirCurtidas': permitirCurtidas,
      'aprovarFotosAntesPublicar': aprovarFotosAntesPublicar,
      'permitirVisualizacaoMural': permitirVisualizacaoMural,
    }..removeWhere((key, value) =>
        value == null || (value is String && value.trim().isEmpty));
  }

  static DateTime _combineDateAndTime(DateTime date, String hhmm) {
    final split = hhmm.split(':');
    final hour = split.isNotEmpty ? int.tryParse(split[0]) ?? 0 : 0;
    final minute = split.length > 1 ? int.tryParse(split[1]) ?? 0 : 0;
    return DateTime(
      date.year,
      date.month,
      date.day,
      hour.clamp(0, 23).toInt(),
      minute.clamp(0, 59).toInt(),
    ).toUtc();
  }

  static int _toApiStatusCode(String? value) {
    final normalized = (value ?? '').trim().toLowerCase();
    return switch (normalized) {
      'published' || 'publicado' || 'confirmado' => 1,
      'cancelled' || 'cancelado' => 2,
      'completed' || 'finalizado' || 'concluido' => 3,
      _ => 0,
    };
  }
}
