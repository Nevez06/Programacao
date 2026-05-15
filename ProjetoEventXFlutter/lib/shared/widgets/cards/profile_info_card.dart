import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';

class ProfileInfoCard extends StatelessWidget {
  const ProfileInfoCard({
    required this.name,
    required this.email,
    required this.roleLabel,
    this.phone,
    this.city,
    this.state,
    this.cpf,
    this.avatarUrl,
    this.actions = const <Widget>[],
    super.key,
  });

  final String name;
  final String email;
  final String roleLabel;
  final String? phone;
  final String? city;
  final String? state;
  final String? cpf;
  final String? avatarUrl;
  final List<Widget> actions;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(EventXSpacing.md),
      decoration: BoxDecoration(
        color: EventXColors.organizerSurface,
        borderRadius: BorderRadius.circular(EventXRadius.lg),
        border: Border.all(color: EventXColors.organizerStroke),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              CircleAvatar(
                radius: 30,
                backgroundImage: (avatarUrl ?? '').isNotEmpty
                    ? NetworkImage(avatarUrl!)
                    : null,
                child: (avatarUrl ?? '').isEmpty
                    ? Text(
                        name.trim().isEmpty
                            ? 'U'
                            : name.trim().characters.first,
                        style: const TextStyle(fontWeight: FontWeight.w700),
                      )
                    : null,
              ),
              const SizedBox(width: EventXSpacing.md),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      name,
                      style: Theme.of(context).textTheme.titleMedium?.copyWith(
                            fontWeight: FontWeight.w700,
                          ),
                    ),
                    const SizedBox(height: 2),
                    Text(email, style: Theme.of(context).textTheme.bodyMedium),
                    const SizedBox(height: 6),
                    Container(
                      padding: const EdgeInsets.symmetric(
                        horizontal: 10,
                        vertical: 4,
                      ),
                      decoration: BoxDecoration(
                        color: EventXColors.brand.withValues(alpha: 0.1),
                        borderRadius: BorderRadius.circular(EventXRadius.pill),
                      ),
                      child: Text(
                        roleLabel,
                        style: const TextStyle(
                          fontSize: 12,
                          color: EventXColors.brand,
                          fontWeight: FontWeight.w700,
                        ),
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
          const SizedBox(height: EventXSpacing.md),
          Wrap(
            spacing: EventXSpacing.sm,
            runSpacing: EventXSpacing.sm,
            children: [
              if ((phone ?? '').isNotEmpty)
                _InfoChip(label: 'Telefone', value: phone!),
              if ((cpf ?? '').isNotEmpty) _InfoChip(label: 'CPF', value: cpf!),
              if ((city ?? '').isNotEmpty || (state ?? '').isNotEmpty)
                _InfoChip(
                  label: 'Local',
                  value:
                      '${city ?? ''}${(city ?? '').isNotEmpty && (state ?? '').isNotEmpty ? '/' : ''}${state ?? ''}',
                ),
            ],
          ),
          if (actions.isNotEmpty) ...[
            const SizedBox(height: EventXSpacing.md),
            Wrap(
              spacing: EventXSpacing.sm,
              runSpacing: EventXSpacing.sm,
              children: actions,
            ),
          ],
        ],
      ),
    );
  }
}

class _InfoChip extends StatelessWidget {
  const _InfoChip({
    required this.label,
    required this.value,
  });

  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 8),
      decoration: BoxDecoration(
        color: EventXColors.organizerSurfaceAlt,
        borderRadius: BorderRadius.circular(EventXRadius.md),
      ),
      child: Text(
        '$label: $value',
        style: const TextStyle(
          fontSize: 12,
          fontWeight: FontWeight.w600,
          color: EventXColors.organizerText,
        ),
      ),
    );
  }
}
