import 'dart:io';

import 'package:dio/dio.dart';
import 'package:dio/io.dart';

void configureBadCertificateSupport(Dio dio) {
  final adapter = IOHttpClientAdapter();
  adapter.createHttpClient = () {
    final client = HttpClient();
    client.badCertificateCallback = (_, __, ___) => true;
    return client;
  };
  dio.httpClientAdapter = adapter;
}
