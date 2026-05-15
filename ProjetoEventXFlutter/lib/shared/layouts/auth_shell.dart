import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/theme/app_design_system.dart';
import 'package:projeto_eventx_flutter/shared/widgets/motion/fade_slide_in.dart';
import 'package:projeto_eventx_flutter/shared/components/cards/app_card.dart';

class AuthShell extends StatelessWidget {
  const AuthShell({
    required this.title,
    required this.subtitle,
    required this.child,
    super.key,
  });

  final String title;
  final String subtitle;
  final Widget child;

  @override
  Widget build(BuildContext context) {
    final isWide = MediaQuery.of(context).size.width >= 980;
    return Scaffold(
      body: Container(
        decoration:
            const BoxDecoration(gradient: AppGradients.organizerSurface),
        child: SafeArea(
          child: Center(
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: 1120),
              child: Row(
                children: [
                  if (isWide)
                    Expanded(
                      child: FadeSlideIn(
                        offset: const Offset(-0.04, 0),
                        child: Padding(
                          padding: const EdgeInsets.all(AppSpacing.xl),
                          child: _AuthBrandPanel(
                            title: title,
                            subtitle: subtitle,
                          ),
                        ),
                      ),
                    ),
                  Expanded(
                    child: FadeSlideIn(
                      offset: const Offset(0.04, 0),
                      child: Padding(
                        padding: const EdgeInsets.all(AppSpacing.md),
                        child: AppCard(
                          padding: const EdgeInsets.all(AppSpacing.lg),
                          child: child,
                        ),
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}

class _AuthBrandPanel extends StatelessWidget {
  const _AuthBrandPanel({
    required this.title,
    required this.subtitle,
  });

  final String title;
  final String subtitle;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(AppSpacing.xl),
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(AppRadius.xl),
        gradient: AppGradients.organizerHero,
        boxShadow: AppShadows.medium,
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Text(
            'EventX',
            style: AppTextStyles.displayMedium.copyWith(
              color: Colors.white,
              fontWeight: FontWeight.w900,
            ),
          ),
          Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                title,
                style: AppTextStyles.displayMedium.copyWith(
                  color: Colors.white,
                  fontSize: 32,
                ),
              ),
              const SizedBox(height: AppSpacing.md),
              Text(
                subtitle,
                style: AppTextStyles.bodyLarge.copyWith(
                  color: Colors.white.withValues(alpha: 0.88),
                  fontSize: 15,
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}
