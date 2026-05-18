class ChatPresenceEvent {
  const ChatPresenceEvent({
    required this.userId,
    this.conversationId,
  });

  final int userId;
  final int? conversationId;
}
