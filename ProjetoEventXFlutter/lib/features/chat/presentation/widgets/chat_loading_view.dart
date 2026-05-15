import 'package:flutter/material.dart';

class ChatLoadingView extends StatelessWidget {
  const ChatLoadingView({
    this.message = 'Carregando chat...',
    super.key,
  });

  final String message;

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          const SizedBox(
            width: 22,
            height: 22,
            child: CircularProgressIndicator(strokeWidth: 2),
          ),
          const SizedBox(height: 10),
          Text(message),
        ],
      ),
    );
  }
}
