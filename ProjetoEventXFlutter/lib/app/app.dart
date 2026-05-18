import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'package:projeto_eventx_flutter/core/api/api_client.dart';
import 'package:projeto_eventx_flutter/core/auth/auth.dart';
import 'package:projeto_eventx_flutter/core/realtime/signalr_service.dart';
import 'package:projeto_eventx_flutter/core/routing/app_router.dart';
import 'package:projeto_eventx_flutter/core/routing/app_routes.dart';
import 'package:projeto_eventx_flutter/core/storage/session_storage.dart';
import 'package:projeto_eventx_flutter/core/theme/app_theme.dart';
import 'package:projeto_eventx_flutter/features/auth/data/auth_repository.dart';
import 'package:projeto_eventx_flutter/features/auth/presentation/auth_controller.dart';
import 'package:projeto_eventx_flutter/features/chat/data/chat_realtime_service.dart';
import 'package:projeto_eventx_flutter/features/chat/data/chat_repository.dart';
import 'package:projeto_eventx_flutter/features/events/data/events_repository.dart';
import 'package:projeto_eventx_flutter/features/events/data/events_service.dart';
import 'package:projeto_eventx_flutter/features/feed/data/event_feed_remote_data_source.dart';
import 'package:projeto_eventx_flutter/features/feed/data/event_feed_repository.dart';
import 'package:projeto_eventx_flutter/features/feed/data/feed_repository.dart';
import 'package:projeto_eventx_flutter/features/invitations/data/invitation_templates_repository.dart';
import 'package:projeto_eventx_flutter/features/invites/data/invites_repository.dart';
import 'package:projeto_eventx_flutter/features/notifications/data/notifications_live_service.dart';
import 'package:projeto_eventx_flutter/features/notifications/data/notifications_repository.dart';
import 'package:projeto_eventx_flutter/features/notifications/presentation/notifications_controller.dart';
import 'package:projeto_eventx_flutter/features/orders/data/orders_repository.dart';
import 'package:projeto_eventx_flutter/features/organizer_dashboard/data/dashboard_repository.dart';
import 'package:projeto_eventx_flutter/features/profile/data/profile_repository.dart';
import 'package:projeto_eventx_flutter/features/social_explore/data/social_explore_repository.dart';
import 'package:projeto_eventx_flutter/features/social_profile/data/social_profile_repository.dart';
import 'package:projeto_eventx_flutter/features/social_story/data/social_story_repository.dart';
import 'package:projeto_eventx_flutter/features/budget/data/quotes_repository.dart';
import 'package:projeto_eventx_flutter/shared/services/chat_repository.dart';
import 'package:projeto_eventx_flutter/shared/services/event_repository.dart';
import 'package:projeto_eventx_flutter/shared/services/feed_ranking_service.dart';
import 'package:projeto_eventx_flutter/shared/services/invitation_repository.dart';
import 'package:projeto_eventx_flutter/shared/services/marketplace_repository.dart';
import 'package:projeto_eventx_flutter/shared/services/marketplace_scoring_service.dart';
import 'package:projeto_eventx_flutter/shared/services/notification_repository.dart';
import 'package:projeto_eventx_flutter/shared/services/recommendation_service.dart';
import 'package:projeto_eventx_flutter/shared/services/ranking_repository.dart';
import 'package:projeto_eventx_flutter/shared/services/social_repository.dart';
import 'package:projeto_eventx_flutter/shared/services/supplier_ranking_service.dart';

class EventXApp extends StatelessWidget {
  const EventXApp({
    required this.apiClient,
    required this.sessionStorage,
    super.key,
  });

  final ApiClient apiClient;
  final SessionStorage sessionStorage;
  static final GlobalKey<NavigatorState> _navigatorKey =
      GlobalKey<NavigatorState>();

  @override
  Widget build(BuildContext context) {
    return MultiProvider(
      providers: [
        Provider<SessionStorage>.value(value: sessionStorage),
        Provider<ApiClient>.value(value: apiClient),
        Provider<TokenStorageService>(
          create: (context) => TokenStorageService(
            context.read<SessionStorage>(),
          ),
        ),
        Provider<SessionService>(
          create: (context) => SessionService(
            context.read<SessionStorage>(),
            context.read<TokenStorageService>(),
          ),
        ),
        Provider<AuthRepository>(
          create: (context) => AuthRepository(
            apiClient: context.read<ApiClient>(),
            sessionService: context.read<SessionService>(),
          ),
        ),
        Provider<AuthService>(
          create: (context) => AuthService(
            context.read<AuthRepository>(),
          ),
        ),
        Provider<EventsService>(
          create: (context) => EventsService(
            sessionStorage: context.read<SessionStorage>(),
          ),
        ),
        Provider<EventsRepository>(
          create: (context) => EventsRepository(
            apiClient: context.read<ApiClient>(),
            eventsService: context.read<EventsService>(),
          ),
        ),
        Provider<QuotesRepository>(
          create: (context) => QuotesRepository(
            apiClient: context.read<ApiClient>(),
          ),
        ),
        Provider<OrdersRepository>(
          create: (context) => OrdersRepository(
            apiClient: context.read<ApiClient>(),
          ),
        ),
        Provider<InvitationTemplatesRepository>(
          create: (context) => InvitationTemplatesRepository(
            apiClient: context.read<ApiClient>(),
          ),
        ),
        Provider<SocialStoryRepository>(
          create: (context) => SocialStoryRepository(
            apiClient: context.read<ApiClient>(),
          ),
        ),
        Provider<SocialProfileRepository>(
          create: (context) => SocialProfileRepository(
            apiClient: context.read<ApiClient>(),
          ),
        ),
        Provider<SocialExploreRepository>(
          create: (context) => SocialExploreRepository(
            apiClient: context.read<ApiClient>(),
          ),
        ),
        Provider<FeedRankingService>(
          create: _createFeedRankingService,
        ),
        Provider<SupplierRankingService>(
          create: _createSupplierRankingService,
        ),
        Provider<MarketplaceScoringService>(
          create: (context) => MarketplaceScoringService(
            rankingService: context.read<SupplierRankingService>(),
          ),
        ),
        Provider<RecommendationService>(
          create: (context) => RecommendationService(
            feedRankingService: context.read<FeedRankingService>(),
            marketplaceScoringService:
                context.read<MarketplaceScoringService>(),
          ),
        ),
        Provider<ProfileRepository>(
          create: (context) => ProfileRepository(
            apiClient: context.read<ApiClient>(),
          ),
        ),
        Provider<DashboardRepository>(
          create: (context) => DashboardRepository(
            eventsRepository: context.read<EventsRepository>(),
            ordersRepository: context.read<OrdersRepository>(),
            quotesRepository: context.read<QuotesRepository>(),
            profileRepository: context.read<ProfileRepository>(),
          ),
        ),
        Provider<FeedRepository>(
          create: (context) => FeedRepository(
            apiClient: context.read<ApiClient>(),
          ),
        ),
        Provider<EventFeedRemoteDataSource>(
          create: (context) => EventFeedRemoteDataSource(
            apiClient: context.read<ApiClient>(),
          ),
        ),
        Provider<EventFeedRepository>(
          create: (context) => EventFeedRepository(
            remoteDataSource: context.read<EventFeedRemoteDataSource>(),
          ),
        ),
        Provider<InvitesRepository>(
          create: (context) => InvitesRepository(
            apiClient: context.read<ApiClient>(),
          ),
        ),
        Provider<ChatRepository>(
          create: (context) => ChatRepository(
            apiClient: context.read<ApiClient>(),
          ),
        ),
        Provider<ChatRealtimeService>(
          create: (context) => ChatRealtimeService(
            apiClient: context.read<ApiClient>(),
          ),
          dispose: (_, value) => value.dispose(),
        ),
        Provider<NotificationsRepository>(
          create: (context) => NotificationsRepository(
            apiClient: context.read<ApiClient>(),
          ),
        ),
        Provider<EventRepository>(
          create: (context) => EventRepository(
            context.read<EventsRepository>(),
          ),
        ),
        Provider<InvitationRepository>(
          create: (context) => InvitationRepository(
            context.read<InvitesRepository>(),
          ),
        ),
        Provider<NotificationRepository>(
          create: (context) => NotificationRepository(
            context.read<NotificationsRepository>(),
          ),
        ),
        Provider<SocialRepository>(
          create: (context) => SocialRepository(
            context.read<FeedRepository>(),
            storyRepository: context.read<SocialStoryRepository>(),
            profileRepository: context.read<SocialProfileRepository>(),
            exploreRepository: context.read<SocialExploreRepository>(),
            rankingService: context.read<FeedRankingService>(),
          ),
        ),
        Provider<ChatFeatureRepository>(
          create: (context) => ChatFeatureRepository(
            context.read<ChatRepository>(),
          ),
        ),
        Provider<MarketplaceRepository>(
          create: (context) => MarketplaceRepository(
            apiClient: context.read<ApiClient>(),
          ),
        ),
        Provider<RankingRepository>(
          create: (context) => RankingRepository(
            apiClient: context.read<ApiClient>(),
          ),
        ),
        Provider<NotificationsLiveService>(
          create: (context) => NotificationsLiveService(
            notificationsRepository: context.read<NotificationsRepository>(),
            signalRFactory: (hubPath) => SignalRService(
              apiClient: context.read<ApiClient>(),
              hubPath: hubPath,
            ),
          ),
          dispose: (_, value) => value.dispose(),
        ),
        ChangeNotifierProvider<NotificationsController>(
          create: (context) => NotificationsController(
            notificationsRepository: context.read<NotificationsRepository>(),
            liveService: context.read<NotificationsLiveService>(),
          )..initialize(),
        ),
        ChangeNotifierProvider<AuthController>(
          create: (context) => AuthController(
            authService: context.read<AuthService>(),
            apiClient: context.read<ApiClient>(),
          )..initialize(),
        ),
      ],
      child: AuthSessionGuard(
        navigatorKey: _navigatorKey,
        child: MaterialApp(
          navigatorKey: _navigatorKey,
          title: 'ProjetoEventX',
          debugShowCheckedModeBanner: false,
          theme: AppTheme.light(),
          initialRoute: AppRoutes.splash,
          onGenerateRoute: AppRouter.generateRoute,
        ),
      ),
    );
  }
}

FeedRankingService _createFeedRankingService(BuildContext _) {
  return const FeedRankingService();
}

SupplierRankingService _createSupplierRankingService(BuildContext _) {
  return const SupplierRankingService();
}
