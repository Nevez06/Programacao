import 'package:projeto_eventx_flutter/core/utils/id_value_parser.dart';

class EventListItem {
  const EventListItem({
    required this.id,
    required this.nomeEvento,
    required this.dataEvento,
    required this.tipoEvento,
    required this.statusEvento,
    required this.publicoEstimado,
    required this.custoEstimado,
    this.localId,
    this.localNome,
    this.slug,
    this.imagemCapa,
  });

  final int id;
  final String nomeEvento;
  final DateTime dataEvento;
  final String tipoEvento;
  final String statusEvento;
  final int publicoEstimado;
  final double custoEstimado;
  final int? localId;
  final String? localNome;
  final String? slug;
  final String? imagemCapa;

  factory EventListItem.fromJson(Map<String, dynamic> json) {
    dynamic read(String key) => json[key] ?? json[_toPascalCase(key)];
    final startDateText =
        (read('dataEvento') ?? read('startDate') ?? '').toString();
    final endDateText = (read('endDate') ?? '').toString();
    final startDate = DateTime.tryParse(startDateText);
    final endDate = DateTime.tryParse(endDateText);
    final eventDate =
        startDate ?? endDate ?? DateTime.fromMillisecondsSinceEpoch(0);
    return EventListItem(
      id: IdValueParser.parseToInt(read('id')),
      nomeEvento: (read('nomeEvento') ?? read('name') ?? '').toString(),
      dataEvento: eventDate,
      tipoEvento: (read('tipoEvento') ?? read('type') ?? '').toString(),
      statusEvento: (read('statusEvento') ?? read('status') ?? '').toString(),
      publicoEstimado: (read('publicoEstimado') as num?)?.toInt() ??
          (read('estimatedAudience') as num?)?.toInt() ??
          0,
      custoEstimado: (read('custoEstimado') as num?)?.toDouble() ??
          (read('estimatedCost') as num?)?.toDouble() ??
          0,
      localId: (read('localId') as num?)?.toInt(),
      localNome: read('localNome')?.toString() ?? read('location')?.toString(),
      slug: read('slug')?.toString(),
      imagemCapa:
          read('imagemCapa')?.toString() ?? read('coverImageUrl')?.toString(),
    );
  }

  static String _toPascalCase(String value) {
    if (value.isEmpty) {
      return value;
    }
    return value[0].toUpperCase() + value.substring(1);
  }
}
