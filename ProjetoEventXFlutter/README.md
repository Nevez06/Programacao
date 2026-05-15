# ProjetoEventX Flutter (Fase 3)

Base Flutter para migracao incremental do sistema ASP.NET MVC + API, mantendo evolucao por modulos.

## Modulos implementados

- Autenticacao
  - `POST /api/auth/login`
  - `GET /api/users/me`
- Eventos
  - `GET /api/events`
  - `GET /api/events/{id}`
- Perfil
  - `GET /api/users/me`
  - `GET /api/users/{id}`
  - `PUT /api/users/me`
- Feed social
  - Estrutura criada no Flutter
  - Integracao pendente de endpoints REST `/api/feed` ou `/api/posts`
- Convites
  - Estrutura criada no Flutter
  - Integracao pendente de endpoints REST `/api/invites`

## Navegacao principal

- Home
- Feed
- Eventos
- Convites
- Perfil

## Rodar

1. Instale Flutter SDK e adicione `flutter` no `PATH`.
2. No diretorio deste projeto, gere os arquivos de plataforma:

```bash
flutter create .
```

3. Instale dependencias e execute:

```bash
flutter pub get
flutter run --dart-define=API_BASE_URL=http://SEU_HOST:PORTA
```

## Observacoes

- Emulador Android com backend local: use `http://10.0.2.2:PORTA`.
- No Web, sem `API_BASE_URL`, o app usa host atual na porta `5163` (ex.: `http://localhost:5163`).
- Backend HTTPS local com certificado de desenvolvimento:

```bash
flutter run --dart-define=API_BASE_URL=https://SEU_HOST:PORTA --dart-define=ALLOW_BAD_CERTS=true
```

- O app usa cookies persistentes e armazenamento local de sessao para evolucao futura.
- Feed e convites aparecem no app com estado pendente, sem mock fake, ate os endpoints API reais serem liberados.
