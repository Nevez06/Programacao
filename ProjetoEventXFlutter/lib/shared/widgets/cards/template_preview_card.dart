import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/template_card.dart';

class TemplatePreviewCard extends StatelessWidget {
  const TemplatePreviewCard({
    required this.name,
    required this.category,
    this.onTap,
    super.key,
  });

  final String name;
  final String category;
  final VoidCallback? onTap;

  @override
  Widget build(BuildContext context) {
    return TemplateCard(name: name, category: category, onTap: onTap);
  }
}
