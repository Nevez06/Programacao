import 'package:projeto_eventx_flutter/core/utils/id_value_parser.dart';

class EventDetails {
  const EventDetails({
    required this.id,
    required this.nomeEvento,
    required this.dataEvento,
    required this.descricaoEvento,
    required this.tipoEvento,
    required this.custoEstimado,
    required this.statusEvento,
    required this.horaInicio,
    required this.horaFim,
    required this.publicoEstimado,
    required this.organizadorId,
    required this.permitirComentarios,
    required this.permitirEnvioFotos,
    required this.permitirCurtidas,
    required this.aprovarFotosAntesPublicar,
    required this.permitirVisualizacaoMural,
    required this.createdAt,
    required this.updatedAt,
    this.localId,
    this.localNome,
    this.localEndereco,
    this.slug,
    this.imagemCapa,
  });

  final int id;
  final String nomeEvento;
  final DateTime dataEvento;
  final String descricaoEvento;
  final String tipoEvento;
  final double custoEstimado;
  final String statusEvento;
  final String horaInicio;
  final String horaFim;
  final int publicoEstimado;
  final int organizadorId;
  final int? localId;
  final String? localNome;
  final String? localEndereco;
  final String? slug;
  final String? imagemCapa;
  final bool permitirComentarios;
  final bool permitirEnvioFotos;
  final bool permitirCurtidas;
  final bool aprovarFotosAntesPublicar;
  final bool permitirVisualizacaoMural;
  final DateTime createdAt;
  final DateTime updatedAt;

  factory EventDetails.fromJson(Map<String, dynamic> json) {
    dynamic read(String key) => json[key] ?? json[_toPascalCase(key)];
    final startDateText =
        (read('dataEvento') ?? read('startDate') ?? '').toString();
    final endDateText = (read('endDate') ?? '').toString();
    final startDate = DateTime.tryParse(startDateText) ??
        DateTime.fromMillisecondsSinceEpoch(0);
    final endDate = DateTime.tryParse(endDateText) ?? startDate;

    return EventDetails(
      id: IdValueParser.parseToInt(read('id')),
      nomeEvento: (read('nomeEvento') ?? read('name') ?? '').toString(),
      dataEvento: startDate,
      descricaoEvento:
          (read('descricaoEvento') ?? read('description') ?? '').toString(),
      tipoEvento: (read('tipoEvento') ?? read('type') ?? '').toString(),
      custoEstimado: (read('custoEstimado') as num?)?.toDouble() ??
          (read('estimatedCost') as num?)?.toDouble() ??
          0,
      statusEvento: (read('statusEvento') ?? read('status') ?? '').toString(),
      horaInicio: (read('horaInicio') ?? _toHourMinute(startDate)).toString(),
      horaFim: (read('horaFim') ?? _toHourMinute(endDate)).toString(),
      publicoEstimado: (read('publicoEstimado') as num?)?.toInt() ??
          (read('estimatedAudience') as num?)?.toInt() ??
          0,
      organizadorId: IdValueParser.parseToInt(
          read('organizadorId') ?? read('organizerId')),
      localId: (read('localId') as num?)?.toInt(),
      localNome: read('localNome')?.toString() ?? read('location')?.toString(),
      localEndereco: read('localEndereco')?.toString(),
      slug: read('slug')?.toString(),
      imagemCapa:
          read('imagemCapa')?.toString() ?? read('coverImageUrl')?.toString(),
      permitirComentarios: read('permitirComentarios') as bool? ?? true,
      permitirEnvioFotos: read('permitirEnvioFotos') as bool? ?? true,
      permitirCurtidas: read('permitirCurtidas') as bool? ?? true,
      aprovarFotosAntesPublicar:
          read('aprovarFotosAntesPublicar') as bool? ?? false,
      permitirVisualizacaoMural:
          read('permitirVisualizacaoMural') as bool? ?? true,
      createdAt: DateTime.tryParse(
              (read('createdAt') ?? read('createdAtUtc') ?? '').toString()) ??
          DateTime.fromMillisecondsSinceEpoch(0),
      updatedAt: DateTime.tryParse(
              (read('updatedAt') ?? read('updatedAtUtc') ?? '').toString()) ??
          DateTime.fromMillisecondsSinceEpoch(0),
    );
  }

  static String _toPascalCase(String value) {
    if (value.isEmpty) {
      return value;
    }
    return value[0].toUpperCase() + value.substring(1);
  }

  static String _toHourMinute(DateTime dateTime) {
    final hh = dateTime.hour.toString().padLeft(2, '0');
    final mm = dateTime.minute.toString().padLeft(2, '0');
    return '$hh:$mm';
  }
}
