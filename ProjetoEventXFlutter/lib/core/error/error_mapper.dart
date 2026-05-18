import 'package:projeto_eventx_flutter/core/error/app_error.dart';

extension AppErrorPresentation on AppError {
  String toUserMessage() {
    return switch (kind) {
      AppErrorKind.unauthorized => 'Sua sessao expirou. Faca login novamente.',
      AppErrorKind.forbidden => 'Voce nao tem permissao para esta acao.',
      AppErrorKind.notFound => 'Recurso nao encontrado.',
      AppErrorKind.timeout =>
        'Tempo limite de conexao atingido. Tente novamente.',
      AppErrorKind.noInternet => 'Sem conexao com a internet.',
      AppErrorKind.server => 'Servidor indisponivel no momento.',
      AppErrorKind.invalidResponse => 'Resposta invalida do servidor.',
      AppErrorKind.unknown => message,
    };
  }
}
