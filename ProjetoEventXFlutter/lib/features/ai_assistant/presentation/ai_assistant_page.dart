import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';
import 'package:projeto_eventx_flutter/routes/app_routes.dart';
import 'package:projeto_eventx_flutter/shared/layouts/organizer_shell.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/action_card.dart';
import 'package:projeto_eventx_flutter/shared/widgets/cards/dashboard_hero_card.dart';
import 'package:projeto_eventx_flutter/shared/widgets/section_header.dart';

class AiAssistantPage extends StatelessWidget {
  const AiAssistantPage({super.key});

  @override
  Widget build(BuildContext context) {
    return OrganizerShell(
      currentRoute: AppRoutes.organizerAiAssistant,
      title: 'IA Assistente',
      subtitle: 'Assistente contextual para operacao do organizador',
      child: ListView(
        padding: const EdgeInsets.all(EventXSpacing.md),
        children: [
          DashboardHeroCard(
            title: 'Assistente EventX ativo',
            subtitle:
                'Sugestoes contextuais para convites, orcamento, pedidos e fornecedores em tempo real.',
            primaryActionLabel: 'Gerar plano da semana',
            onPrimaryAction: () {},
            secondaryActionLabel: 'Abrir convites',
            onSecondaryAction: () =>
                Navigator.of(context).pushNamed(AppRoutes.organizerInvitations),
          ),
          const SizedBox(height: EventXSpacing.lg),
          const SectionHeader(
            title: 'Conversa e contexto',
            subtitle: 'Chat operacional + painel lateral de acoes recomendadas',
          ),
          const SizedBox(height: EventXSpacing.sm),
          LayoutBuilder(
            builder: (context, constraints) {
              final isWide = constraints.maxWidth >= 1060;

              if (isWide) {
                return Row(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: const [
                    Expanded(flex: 3, child: _ChatPanel()),
                    SizedBox(width: EventXSpacing.md),
                    Expanded(flex: 2, child: _AssistantContextPanel()),
                  ],
                );
              }

              return const Column(
                children: [
                  _ChatPanel(),
                  SizedBox(height: EventXSpacing.sm),
                  _AssistantContextPanel(),
                ],
              );
            },
          ),
        ],
      ),
    );
  }
}

class _ChatPanel extends StatelessWidget {
  const _ChatPanel();

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(EventXSpacing.md),
      decoration: BoxDecoration(
        color: EventXColors.organizerSurface,
        borderRadius: BorderRadius.circular(EventXRadius.lg),
        border: Border.all(color: EventXColors.organizerStroke),
      ),
      child: const Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          _AiBubble(
            label: 'IA',
            message:
                'Posso montar um plano de acao para os proximos 7 dias do seu evento principal.',
          ),
          SizedBox(height: EventXSpacing.xs),
          _AiBubble(
            label: 'Voce',
            message: 'Priorize convites e custo de buffet.',
            user: true,
          ),
          SizedBox(height: EventXSpacing.xs),
          _AiBubble(
            label: 'IA',
            message:
                'Perfeito. Sugestao: 1) fechar template, 2) disparar lote inicial, 3) revisar propostas de buffet premium.',
          ),
          SizedBox(height: EventXSpacing.sm),
          _InputStub(),
        ],
      ),
    );
  }
}

class _AssistantContextPanel extends StatelessWidget {
  const _AssistantContextPanel();

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        ActionCard(
          title: 'Acoes rapidas',
          description: 'Gerar checklist, cronograma e lista de riscos.',
          icon: Icons.checklist_rounded,
          buttonLabel: 'Executar',
          onTap: () {},
        ),
        const SizedBox(height: EventXSpacing.sm),
        ActionCard(
          title: 'Comparar fornecedores',
          description: 'Cruze preco, SLA e reputacao para decidir mais rapido.',
          icon: Icons.compare_arrows_rounded,
          buttonLabel: 'Comparar',
          onTap: () =>
              Navigator.of(context).pushNamed(AppRoutes.organizerMarketplace),
          highlightColor: const Color(0xFF2D7DF6),
        ),
        const SizedBox(height: EventXSpacing.sm),
        ActionCard(
          title: 'Resumo de notificacoes',
          description: 'Veja alertas criticos e pendencias de hoje.',
          icon: Icons.notifications_active_outlined,
          buttonLabel: 'Abrir',
          onTap: () =>
              Navigator.of(context).pushNamed(AppRoutes.organizerNotifications),
          highlightColor: const Color(0xFFB9691A),
        ),
      ],
    );
  }
}

class _AiBubble extends StatelessWidget {
  const _AiBubble({
    required this.label,
    required this.message,
    this.user = false,
  });

  final String label;
  final String message;
  final bool user;

  @override
  Widget build(BuildContext context) {
    final bg = user ? const Color(0xFFEFF2FA) : const Color(0xFFF9EFEF);
    return Align(
      alignment: user ? Alignment.centerRight : Alignment.centerLeft,
      child: ConstrainedBox(
        constraints: const BoxConstraints(maxWidth: 620),
        child: Container(
          padding: const EdgeInsets.all(EventXSpacing.sm),
          decoration: BoxDecoration(
            color: bg,
            borderRadius: BorderRadius.circular(EventXRadius.md),
          ),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(label, style: const TextStyle(fontWeight: FontWeight.w700)),
              const SizedBox(height: 2),
              Text(message),
            ],
          ),
        ),
      ),
    );
  }
}

class _InputStub extends StatelessWidget {
  const _InputStub();

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
      decoration: BoxDecoration(
        color: EventXColors.organizerSurfaceAlt,
        borderRadius: BorderRadius.circular(EventXRadius.pill),
      ),
      child: const Row(
        children: [
          Icon(Icons.chat_bubble_outline_rounded,
              size: 18, color: EventXColors.organizerTextMuted),
          SizedBox(width: 8),
          Expanded(
            child: Text(
              'Digite uma pergunta para o assistente...',
              style: TextStyle(color: EventXColors.organizerTextMuted),
            ),
          ),
          Icon(Icons.send_rounded, color: EventXColors.brand),
        ],
      ),
    );
  }
}
