class ChatSendMessageRequest {
  const ChatSendMessageRequest({
    required this.conversationId,
    required this.content,
  });

  final int conversationId;
  final String content;

  Map<String, dynamic> toJson() {
    return {
      'conversationId': conversationId,
      'content': content,
    };
  }
}
