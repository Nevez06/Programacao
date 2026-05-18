class AppRoutes {
  // Core
  static const splash = '/';
  static const landing = '/landing';
  static const home = '/home';

  // Auth
  static const login = '/login';
  static const register = '/register';
  static const loginOrganizer = '/login-organizer';
  static const loginGuest = '/login-guest';
  static const loginSupplier = '/login-supplier';
  static const loginVendor = loginSupplier;

  // Organizer world (SaaS / operational)
  static const organizerDashboard = '/dashboard';
  static const organizerEvents = '/events';
  static const organizerCreateEvent = '/events/create';
  static const organizerInvitations = '/invitations';
  static const organizerTemplateGallery = '/invitations/templates';
  static const organizerEditor = '/editor';
  static const organizerBudget = '/budget';
  static const organizerQuotes = '/budget/quotes';
  static const organizerOrders = '/orders';
  static const organizerMarketplace = '/marketplace';
  static const organizerRanking = '/ranking';
  static const organizerSupplierReview = '/reviews';
  static const organizerReviews = organizerSupplierReview;
  static const organizerAiAssistant = '/assistant';
  static const organizerNotifications = '/notifications';
  static const organizerProfileAdmin = '/profile-admin';

  // Social world (isolated UX)
  static const social = '/social';
  static const socialFeed = '/social/feed';
  static const socialInteractions = '/social/interactions';
  static const socialCreateStory = '/social/story/create';
  static const socialStoryView = '/social/story/view';
  static const socialStories = socialStoryView;
  static const socialHighlights = '/social/highlights';
  static const socialExplore = '/social/explore';
  static const socialProfile = '/social/profile/me';
  static const socialUserProfile = '/social/profile/:username';

  // Existing feature routes kept for compatibility
  static const feed = '/feed';
  static const chat = '/chat';
  static const chatConversation = '/chat/conversation';
  static const postCreate = '/posts/create';
  static const postDetails = '/posts/details';
  static const events = '/legacy/events';
  static const eventDetails = '/events/details';
  static const profile = '/legacy/profile';
  static const profileEdit = '/legacy/profile/edit';
  static const invites = '/invites';
  static const inviteCreate = '/invites/create';
  static const inviteDetails = '/invites/details';
  static const notifications = '/legacy/notifications';

  // Legacy aliases (safe deep-link compatibility)
  static const legacyLoginOrganizer = '/auth/organizer/login';
  static const legacyLoginGuest = '/auth/guest/login';
  static const legacyLoginVendor = '/auth/vendor/login';
  static const legacyOrganizerDashboard = '/organizer/dashboard';
  static const legacyOrganizerEvents = '/organizer/events';
  static const legacyOrganizerCreateEvent = '/organizer/events/create';
  static const legacyOrganizerInvitations = '/organizer/invitations';
  static const legacyOrganizerTemplateGallery =
      '/organizer/invitations/templates';
  static const legacyOrganizerEditor = '/organizer/editor';
  static const legacyOrganizerBudget = '/organizer/budget';
  static const legacyOrganizerQuotes = '/organizer/budget/quotes';
  static const legacyOrganizerOrders = '/organizer/orders';
  static const legacyOrganizerMarketplace = '/organizer/marketplace';
  static const legacyOrganizerRanking = '/organizer/ranking';
  static const legacyOrganizerReviews = '/organizer/ranking/review';
  static const legacyOrganizerAiAssistant = '/organizer/ai-assistant';
  static const legacyOrganizerNotifications = '/organizer/notifications';
  static const legacyOrganizerProfileAdmin = '/organizer/profile-admin';
  static const legacySocialStories = '/social/stories';
  static const legacySocialCreateStory = '/social/stories/create';

  static String socialProfileByUsername(String username) {
    final clean = username.trim().replaceAll('@', '');
    return '/social/profile/$clean';
  }

  static bool isDynamicSocialProfileRoute(String? route) {
    if (route == null || route == socialProfile || route == socialUserProfile) {
      return false;
    }
    return route.startsWith('/social/profile/');
  }

  static String homeForTipoUsuario(String? tipoUsuario) {
    final normalized = (tipoUsuario ?? '').trim().toLowerCase();
    return switch (normalized) {
      'guest' => socialFeed,
      'convidado' => socialFeed,
      'supplier' => organizerMarketplace,
      'fornecedor' => organizerMarketplace,
      'organizer' => organizerDashboard,
      _ => organizerDashboard,
    };
  }
}
