class ApiEndpoints {
  static const auth = '/api/auth';
  static const login = '/api/auth/login';
  static const register = '/api/auth/register';
  static const authMe = '/api/auth/me';
  static const logout = '/api/auth/logout';
  static const me = '/api/users/me';
  static const users = '/api/users';
  static const events = '/api/events';
  static const feed = '/api/feed';
  static const feedPosts = '$feed/posts';
  static const posts = '/api/posts';
  static const social = '/api/social';
  static const socialFeed = '$social/feed';
  static const socialPosts = '$social/posts';
  static const invites = '/api/invites';
  static const invitations = '/api/invitations';
  static const notifications = '/api/notifications';
  static const chat = '/api/chat';
  static const stories = '/api/stories';
  static const highlights = '/api/highlights';
  static const highlightsMe = '/api/highlights/me';
  static const explore = '/api/explore';
  static const exploreTrending = '/api/explore/trending';
  static const exploreProfiles = '/api/explore/profiles';
  static const socialProfile = '/api/social/profile';
  static const socialProfileMe = '$socialProfile/me';
  static const marketplace = '/api/marketplace';
  static const marketplaceSuppliers = '$marketplace/suppliers';
  static const marketplaceCategories = '$marketplace/categories';
  static const quotes = '/api/quotes';
  static const orders = '/api/orders';
  static const rankingSuppliers = '/api/ranking/suppliers';
  static const rankingSuppliersTop = '/api/ranking/suppliers/top';

  static const invitationTemplates = '$invitations/templates';
  static const invitationDrafts = '$invitations/drafts';
  static const invitationCreateFromTemplate =
      '$invitations/create-from-template';

  static const chatConversations = '$chat/conversations';
  static const chatMessages = '$chat/messages';
  static const notificationsUnreadCount = '$notifications/unread-count';
  static const notificationsReadAll = '$notifications/read-all';

  static String eventById(int id) => '/api/events/$id';
  static String userById(int id) => '$users/$id';
  static String postById(int id) => '$posts/$id';
  static String postLike(int id) => '${postById(id)}/like';
  static String postComment(int id) => '${postById(id)}/comment';
  static String socialPostById(int id) => '$socialPosts/$id';
  static String socialPostLike(int id) => '${socialPostById(id)}/like';
  static String socialPostComments(int id) => '${socialPostById(id)}/comments';
  static String storyById(int id) => '$stories/$id';
  static String storyView(int id) => '${storyById(id)}/view';
  static String inviteById(int id) => '$invites/$id';
  static String inviteRsvp(int id) => '${inviteById(id)}/rsvp';
  static String inviteSend(int id) => '${inviteById(id)}/send';
  static String socialProfileById(int id) => '$socialProfile/$id';
  static String socialProfileByUsername(String username) =>
      '$socialProfile/username/${username.trim().replaceAll('@', '')}';
  static String socialProfilePosts(int id) => '${socialProfileById(id)}/posts';
  static String socialProfileStories(int id) =>
      '${socialProfileById(id)}/stories';
  static String socialFollow(int id) => '${socialProfileById(id)}/follow';
  static String socialFollowers(int id) => '${socialProfileById(id)}/followers';
  static String socialFollowing(int id) => '${socialProfileById(id)}/following';
  static String marketplaceSupplierById(int id) => '$marketplaceSuppliers/$id';
  static String quoteById(int id) => '$quotes/$id';
  static String quoteStatus(int id) => '${quoteById(id)}/status';
  static String quoteNegotiate(int id) => '${quoteById(id)}/negotiate';
  static String orderById(String id) => '$orders/$id';
  static String orderStatus(String id) => '${orderById(id)}/status';
  static String rankingSupplierById(int supplierId) =>
      '$rankingSuppliers/$supplierId';
  static String notificationRead(int id) => '$notifications/$id/read';
  static String invitationDraftById(int id) => '$invitationDrafts/$id';
  static String chatMessagesByConversation(int conversationId) =>
      '$chatConversations/$conversationId/messages';
}
