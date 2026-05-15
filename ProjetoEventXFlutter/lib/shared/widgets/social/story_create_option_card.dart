import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';

class StoryCreateOptionCard extends StatelessWidget {
  const StoryCreateOptionCard({
    required this.title,
    required this.subtitle,
    required this.icon,
    required this.gradient,
    this.onTap,
    super.key,
  });

  final String title;
  final String subtitle;
  final IconData icon;
  final List<Color> gradient;
  final VoidCallback? onTap;

  @override
  Widget build(BuildContext context) {
    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(EventXRadius.lg),
      child: Ink(
        padding: const EdgeInsets.all(EventXSpacing.md),
        decoration: BoxDecoration(
          gradient: LinearGradient(
            colors: gradient,
            begin: Alignment.topLeft,
            end: Alignment.bottomRight,
          ),
          borderRadius: BorderRadius.circular(EventXRadius.lg),
          border: Border.all(color: EventXColors.socialStroke),
          boxShadow: EventXShadows.soft,
        ),
        child: Row(
          children: [
            Container(
              width: 44,
              height: 44,
              decoration: BoxDecoration(
                color: Colors.white.withValues(alpha: 0.16),
                borderRadius: BorderRadius.circular(EventXRadius.md),
              ),
              alignment: Alignment.center,
              child: Icon(icon, color: Colors.white),
            ),
            const SizedBox(width: EventXSpacing.sm),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    title,
                    style: const TextStyle(
                      color: Colors.white,
                      fontWeight: FontWeight.w800,
                      fontSize: 15,
                    ),
                  ),
                  Text(
                    subtitle,
                    style: const TextStyle(
                      color: Color(0xFFE4E7F9),
                      fontSize: 12,
                    ),
                  ),
                ],
              ),
            ),
            const Icon(Icons.chevron_right_rounded, color: Colors.white),
          ],
        ),
      ),
    );
  }
}
