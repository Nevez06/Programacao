import 'package:projeto_eventx_flutter/core/api/api_client.dart';
import 'package:projeto_eventx_flutter/core/constants/api_endpoints.dart';
import 'package:projeto_eventx_flutter/features/budget/data/models/quote_model.dart';

class QuotesRepository {
  QuotesRepository({
    required ApiClient apiClient,
  }) : _apiClient = apiClient;

  final ApiClient _apiClient;

  Future<List<QuoteModel>> getQuotes({String? status, int? eventId}) async {
    final list = await _apiClient.getList(
      ApiEndpoints.quotes,
      queryParameters: {
        if (status != null && status.trim().isNotEmpty) 'status': status,
        if (eventId != null) 'eventId': eventId,
      },
    );

    final quotes = <QuoteModel>[];
    for (final item in list) {
      if (item is Map<String, dynamic>) {
        quotes.add(QuoteModel.fromJson(item));
        continue;
      }
      if (item is Map) {
        quotes.add(QuoteModel.fromJson(Map<String, dynamic>.from(item)));
      }
    }
    return quotes;
  }

  Future<QuoteModel> getQuoteById(int id) async {
    final json = await _apiClient.getJson(ApiEndpoints.quoteById(id));
    return QuoteModel.fromJson(json);
  }

  Future<QuoteModel> createQuote(CreateQuoteRequest request) async {
    final json = await _apiClient.postJson(
      ApiEndpoints.quotes,
      body: request.toJson(),
    );
    return QuoteModel.fromJson(json);
  }

  Future<QuoteModel> updateStatus(
    int id,
    UpdateQuoteStatusRequest request,
  ) async {
    final json = await _apiClient.putJson(
      ApiEndpoints.quoteStatus(id),
      body: request.toJson(),
    );
    return QuoteModel.fromJson(json);
  }

  Future<QuoteModel> negotiate(
    int id,
    NegotiateQuoteRequest request,
  ) async {
    final json = await _apiClient.putJson(
      ApiEndpoints.quoteNegotiate(id),
      body: request.toJson(),
    );
    return QuoteModel.fromJson(json);
  }
}
