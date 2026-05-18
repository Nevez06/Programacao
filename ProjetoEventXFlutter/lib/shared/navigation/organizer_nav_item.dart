import 'package:flutter/material.dart';

class OrganizerNavItem {
  const OrganizerNavItem({
    required this.route,
    required this.label,
    required this.icon,
  });

  final String route;
  final String label;
  final IconData icon;
}
