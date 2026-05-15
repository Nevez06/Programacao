import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/theme/app_shadows.dart';

class EventXShadows {
  EventXShadows._();

  static List<BoxShadow> get card => AppShadows.medium;
  static List<BoxShadow> get soft => AppShadows.light;
  static List<BoxShadow> get heavy => AppShadows.heavy;
}
