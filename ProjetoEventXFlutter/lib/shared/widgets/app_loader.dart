import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/shared/widgets/loading_view.dart';

class AppLoader extends StatelessWidget {
  const AppLoader({super.key, this.message});

  final String? message;

  @override
  Widget build(BuildContext context) {
    return LoadingView(message: message);
  }
}
