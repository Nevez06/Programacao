import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/theme/app_motion.dart';
import 'package:projeto_eventx_flutter/features/ai_assistant/presentation/ai_assistant_page.dart';
import 'package:projeto_eventx_flutter/features/auth/presentation/login_page.dart';
import 'package:projeto_eventx_flutter/features/auth/presentation/register_page.dart';
import 'package:projeto_eventx_flutter/features/budget/presentation/budget_overview_page.dart';
import 'package:projeto_eventx_flutter/features/budget/presentation/quotes_page.dart';
import 'package:projeto_eventx_flutter/features/chat/presentation/chat_conversation_page.dart';
import 'package:projeto_eventx_flutter/features/chat/presentation/chat_list_page.dart';
import 'package:projeto_eventx_flutter/features/editor/presentation/eventx_editor_page.dart';
import 'package:projeto_eventx_flutter/features/events/presentation/create_event_page.dart';
import 'package:projeto_eventx_flutter/features/events/presentation/event_details_page.dart';
import 'package:projeto_eventx_flutter/features/events/presentation/events_page.dart';
import 'package:projeto_eventx_flutter/features/events/presentation/organizer_events_page.dart';
import 'package:projeto_eventx_flutter/features/feed/presentation/create_post_page.dart';
import 'package:projeto_eventx_flutter/features/feed/presentation/feed_page.dart';
import 'package:projeto_eventx_flutter/features/feed/presentation/post_details_page.dart';
import 'package:projeto_eventx_flutter/features/invitations/presentation/invitations_hub_page.dart';
import 'package:projeto_eventx_flutter/features/invitations/presentation/template_gallery_page.dart';
import 'package:projeto_eventx_flutter/features/invitations/data/models/invitation_draft_model.dart';
import 'package:projeto_eventx_flutter/features/invites/presentation/create_invite_page.dart';
import 'package:projeto_eventx_flutter/features/invites/presentation/invite_details_page.dart';
import 'package:projeto_eventx_flutter/features/invites/presentation/invites_page.dart';
import 'package:projeto_eventx_flutter/features/landing/presentation/landing_page.dart';
import 'package:projeto_eventx_flutter/features/marketplace/presentation/marketplace_page.dart';
import 'package:projeto_eventx_flutter/features/notifications/presentation/notifications_page.dart';
import 'package:projeto_eventx_flutter/features/notifications/presentation/organizer_notifications_page.dart';
import 'package:projeto_eventx_flutter/features/orders/presentation/orders_page.dart';
import 'package:projeto_eventx_flutter/features/organizer_dashboard/presentation/organizer_dashboard_page.dart';
import 'package:projeto_eventx_flutter/features/profile/presentation/edit_profile_page.dart';
import 'package:projeto_eventx_flutter/features/profile/presentation/profile_page.dart';
import 'package:projeto_eventx_flutter/features/profile_admin/presentation/profile_admin_page.dart';
import 'package:projeto_eventx_flutter/features/ranking/presentation/ranking_page.dart';
import 'package:projeto_eventx_flutter/features/ranking/presentation/supplier_review_page.dart';
import 'package:projeto_eventx_flutter/features/social_explore/presentation/social_explore_page.dart';
import 'package:projeto_eventx_flutter/features/social_feed/presentation/social_feed_page.dart';
import 'package:projeto_eventx_flutter/features/social_feed/presentation/social_interactions_page.dart';
import 'package:projeto_eventx_flutter/features/social_profile/presentation/social_profile_page.dart';
import 'package:projeto_eventx_flutter/features/social_profile/presentation/social_user_profile_page.dart';
import 'package:projeto_eventx_flutter/features/social_story/presentation/create_story_page.dart';
import 'package:projeto_eventx_flutter/features/social_story/presentation/social_stories_page.dart';
import 'package:projeto_eventx_flutter/features/social_story/presentation/story_highlights_page.dart';
import 'package:projeto_eventx_flutter/features/splash/presentation/splash_page.dart';
import 'package:projeto_eventx_flutter/routes/app_routes.dart';

class EventDetailsRouteArgs {
  const EventDetailsRouteArgs(this.eventId);
  final int eventId;
}

class ChatConversationRouteArgs {
  const ChatConversationRouteArgs({
    required this.eventoId,
    required this.titulo,
  });

  final int eventoId;
  final String titulo;
}

class PostDetailsRouteArgs {
  const PostDetailsRouteArgs(this.postId);
  final int postId;
}

class InviteDetailsRouteArgs {
  const InviteDetailsRouteArgs(this.inviteId);
  final int inviteId;
}

class CreateEventRouteArgs {
  const CreateEventRouteArgs({this.eventId});
  final int? eventId;
}

class EventXEditorRouteArgs {
  const EventXEditorRouteArgs({this.draft});
  final InvitationDraftModel? draft;
}

class AppRouter {
  static Route<dynamic> generateRoute(RouteSettings settings) {
    if (AppRoutes.isDynamicSocialProfileRoute(settings.name)) {
      final username = settings.name!.replaceFirst('/social/profile/', '');
      if (username.isEmpty) {
        return _errorRoute('Perfil social invalido.');
      }
      return _build(
        SocialUserProfilePage(username: '@$username'),
        settings,
      );
    }

    switch (settings.name) {
      case AppRoutes.splash:
        return _build(const SplashPage(), settings);
      case AppRoutes.landing:
        return _build(const LandingPage(), settings);

      case AppRoutes.login:
      case AppRoutes.loginOrganizer:
      case AppRoutes.legacyLoginOrganizer:
        return _build(
          const LoginPage(mode: LoginMode.organizer),
          settings,
        );
      case AppRoutes.loginGuest:
      case AppRoutes.legacyLoginGuest:
        return _build(
          const LoginPage(mode: LoginMode.guest),
          settings,
        );
      case AppRoutes.loginSupplier:
      case AppRoutes.legacyLoginVendor:
        return _build(
          const LoginPage(mode: LoginMode.vendor),
          settings,
        );
      case AppRoutes.register:
        return _build(const RegisterPage(), settings);

      case AppRoutes.home:
      case AppRoutes.organizerDashboard:
      case AppRoutes.legacyOrganizerDashboard:
        return _build(const OrganizerDashboardPage(), settings);
      case AppRoutes.organizerEvents:
      case AppRoutes.legacyOrganizerEvents:
        return _build(const OrganizerEventsPage(), settings);
      case AppRoutes.organizerCreateEvent:
      case AppRoutes.legacyOrganizerCreateEvent:
        final createEventArgs = settings.arguments;
        if (createEventArgs is CreateEventRouteArgs) {
          return _build(
            CreateEventPage(eventId: createEventArgs.eventId),
            settings,
          );
        }
        return _build(const CreateEventPage(), settings);
      case AppRoutes.organizerInvitations:
      case AppRoutes.legacyOrganizerInvitations:
        return _build(const InvitationsHubPage(), settings);
      case AppRoutes.organizerTemplateGallery:
      case AppRoutes.legacyOrganizerTemplateGallery:
        return _build(const TemplateGalleryPage(), settings);
      case AppRoutes.organizerEditor:
      case AppRoutes.legacyOrganizerEditor:
        final editorArgs = settings.arguments;
        if (editorArgs is EventXEditorRouteArgs) {
          return _build(
            EventXEditorPage(initialDraft: editorArgs.draft),
            settings,
          );
        }
        return _build(const EventXEditorPage(), settings);
      case AppRoutes.organizerBudget:
      case AppRoutes.legacyOrganizerBudget:
        return _build(const BudgetOverviewPage(), settings);
      case AppRoutes.organizerQuotes:
      case AppRoutes.legacyOrganizerQuotes:
        return _build(const QuotesPage(), settings);
      case AppRoutes.organizerOrders:
      case AppRoutes.legacyOrganizerOrders:
        return _build(const OrdersPage(), settings);
      case AppRoutes.organizerMarketplace:
      case AppRoutes.legacyOrganizerMarketplace:
        return _build(const MarketplacePage(), settings);
      case AppRoutes.organizerRanking:
      case AppRoutes.legacyOrganizerRanking:
        return _build(const RankingPage(), settings);
      case AppRoutes.organizerReviews:
      case AppRoutes.legacyOrganizerReviews:
        return _build(const SupplierReviewPage(), settings);
      case AppRoutes.organizerAiAssistant:
      case AppRoutes.legacyOrganizerAiAssistant:
        return _build(const AiAssistantPage(), settings);
      case AppRoutes.organizerNotifications:
      case AppRoutes.legacyOrganizerNotifications:
        return _build(const OrganizerNotificationsPage(), settings);
      case AppRoutes.organizerProfileAdmin:
      case AppRoutes.legacyOrganizerProfileAdmin:
        return _build(const ProfileAdminPage(), settings);

      case AppRoutes.social:
      case AppRoutes.socialFeed:
        return _build(const SocialFeedPage(), settings);
      case AppRoutes.socialInteractions:
        return _build(const SocialInteractionsPage(), settings);
      case AppRoutes.socialExplore:
        return _build(const SocialExplorePage(), settings);
      case AppRoutes.socialStoryView:
      case AppRoutes.legacySocialStories:
        final storyArgs = settings.arguments;
        if (storyArgs is SocialStoryViewerArgs) {
          return _build(
            SocialStoriesPage(
              initialIndex: storyArgs.initialIndex,
              storyId: storyArgs.storyId,
            ),
            settings,
          );
        }
        return _build(const SocialStoriesPage(), settings);
      case AppRoutes.socialCreateStory:
      case AppRoutes.legacySocialCreateStory:
        return _build(const CreateStoryPage(), settings);
      case AppRoutes.socialHighlights:
        return _build(const StoryHighlightsPage(), settings);
      case AppRoutes.socialProfile:
        return _build(const SocialProfilePage(), settings);
      case AppRoutes.socialUserProfile:
        final args = settings.arguments;
        if (args is SocialUserProfileArgs) {
          return _build(
            SocialUserProfilePage(username: args.username),
            settings,
          );
        }
        return _errorRoute('Parametro de perfil social invalido.');

      // Legacy routes preserved
      case AppRoutes.feed:
        return _build(const FeedPage(), settings);
      case AppRoutes.chat:
        return _build(const ChatListPage(), settings);
      case AppRoutes.chatConversation:
        final chatArgs = settings.arguments;
        if (chatArgs is ChatConversationRouteArgs) {
          return _build(
            ChatConversationPage(
              eventoId: chatArgs.eventoId,
              titulo: chatArgs.titulo,
            ),
            settings,
          );
        }
        return _errorRoute('Parametro de conversa invalido.');
      case AppRoutes.postCreate:
        return _build(const CreatePostPage(), settings);
      case AppRoutes.postDetails:
        final postArgs = settings.arguments;
        if (postArgs is PostDetailsRouteArgs) {
          return _build(PostDetailsPage(postId: postArgs.postId), settings);
        }
        return _errorRoute('Parametro de post invalido.');
      case AppRoutes.events:
        return _build(const EventsPage(), settings);
      case AppRoutes.eventDetails:
        final eventArgs = settings.arguments;
        if (eventArgs is EventDetailsRouteArgs) {
          return _build(EventDetailsPage(eventId: eventArgs.eventId), settings);
        }
        return _errorRoute('Parametro de evento invalido.');
      case AppRoutes.profile:
        return _build(const ProfilePage(), settings);
      case AppRoutes.profileEdit:
        return _build(const EditProfilePage(), settings);
      case AppRoutes.invites:
        return _build(const InvitesPage(), settings);
      case AppRoutes.inviteCreate:
        return _build(const CreateInvitePage(), settings);
      case AppRoutes.inviteDetails:
        final inviteArgs = settings.arguments;
        if (inviteArgs is InviteDetailsRouteArgs) {
          return _build(
              InviteDetailsPage(inviteId: inviteArgs.inviteId), settings);
        }
        return _errorRoute('Parametro de convite invalido.');
      case AppRoutes.notifications:
        return _build(const NotificationsPage(), settings);
      default:
        return _errorRoute('Rota nao encontrada: ${settings.name}');
    }
  }

  static Route<dynamic> _build(Widget page, RouteSettings settings) {
    return AppMotion.buildRoute<dynamic>(
      page: page,
      settings: settings,
      style: _resolveRouteTransition(settings.name),
    );
  }

  static Route<dynamic> _errorRoute(String message) {
    return MaterialPageRoute<void>(
      builder: (_) => Scaffold(
        appBar: AppBar(title: const Text('Erro')),
        body: Center(
          child: Padding(
            padding: const EdgeInsets.all(16),
            child: Text(message),
          ),
        ),
      ),
    );
  }

  static AppRouteTransitionStyle _resolveRouteTransition(String? routeName) {
    if (routeName == null) {
      return AppRouteTransitionStyle.organizer;
    }
    if (_isAuthRoute(routeName)) {
      return AppRouteTransitionStyle.auth;
    }
    if (_isSocialRoute(routeName)) {
      return AppRouteTransitionStyle.social;
    }
    return AppRouteTransitionStyle.organizer;
  }

  static bool _isAuthRoute(String routeName) {
    return routeName == AppRoutes.splash ||
        routeName == AppRoutes.landing ||
        routeName == AppRoutes.login ||
        routeName == AppRoutes.register ||
        routeName == AppRoutes.loginGuest ||
        routeName == AppRoutes.loginOrganizer ||
        routeName == AppRoutes.loginSupplier ||
        routeName.startsWith('/auth/');
  }

  static bool _isSocialRoute(String routeName) {
    return routeName.startsWith('/social') ||
        routeName == AppRoutes.feed ||
        routeName == AppRoutes.postCreate ||
        routeName == AppRoutes.postDetails;
  }
}
