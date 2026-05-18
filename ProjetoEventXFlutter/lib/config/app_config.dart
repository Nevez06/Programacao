class AppConfig {
  static const String _fixedApiBaseUrl = 'http://127.0.0.1:5163';

  static String get baseUrl {
    return _normalizeBaseUrl(_fixedApiBaseUrl);
  }

  static String get apiBaseUrl => baseUrl;

  static const allowBadCertificates = bool.fromEnvironment(
    'ALLOW_BAD_CERTS',
    defaultValue: false,
  );

  static const chatHubPath = String.fromEnvironment(
    'CHAT_HUB_PATH',
    defaultValue: '/chatHub',
  );

  // Quando vazio, notificacoes em tempo real usam fallback por polling.
  static const notificationsHubPath = String.fromEnvironment(
    'NOTIFICATIONS_HUB_PATH',
    defaultValue: '',
  );

  static const notificationsPollingSeconds = int.fromEnvironment(
    'NOTIFICATIONS_POLLING_SECONDS',
    defaultValue: 25,
  );

  static String _normalizeBaseUrl(String raw) {
    final trimmed = raw.trim();
    if (trimmed.isEmpty) {
      return trimmed;
    }
    final withScheme =
        trimmed.startsWith('http://') || trimmed.startsWith('https://')
            ? trimmed
            : 'http://$trimmed';
    return withScheme.endsWith('/')
        ? withScheme.substring(0, withScheme.length - 1)
        : withScheme;
  }
}
