import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';

class SocialTabBar extends StatelessWidget {
  const SocialTabBar({required this.tabs, super.key});

  final List<String> tabs;

  @override
  Widget build(BuildContext context) {
    return Container(
      height: 44,
      padding: const EdgeInsets.all(4),
      decoration: BoxDecoration(
        color: EventXColors.socialSurface,
        borderRadius: BorderRadius.circular(EventXRadius.pill),
        border: Border.all(color: EventXColors.socialStroke),
      ),
      child: TabBar(
        dividerColor: Colors.transparent,
        indicatorSize: TabBarIndicatorSize.tab,
        indicator: BoxDecoration(
          color: EventXColors.socialSurfaceAlt,
          borderRadius: BorderRadius.circular(EventXRadius.pill),
        ),
        labelColor: EventXColors.socialText,
        unselectedLabelColor: EventXColors.socialTextMuted,
        labelStyle: const TextStyle(fontWeight: FontWeight.w700),
        tabs: tabs.map((tab) => Tab(text: tab)).toList(),
      ),
    );
  }
}
