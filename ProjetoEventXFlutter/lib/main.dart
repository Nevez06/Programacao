import 'package:flutter/widgets.dart';
import 'package:projeto_eventx_flutter/app/app.dart';
import 'package:projeto_eventx_flutter/core/api/api_client.dart';
import 'package:projeto_eventx_flutter/core/storage/session_storage.dart';

Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();

  final sessionStorage = await SessionStorage.create();
  final apiClient = await ApiClient.create(sessionStorage: sessionStorage);

  runApp(EventXApp(apiClient: apiClient, sessionStorage: sessionStorage));
}
