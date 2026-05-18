import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/core/design_system/eventx_design_system.dart';
import 'package:projeto_eventx_flutter/shared/widgets/social/profile_stats_row.dart';

class SocialProfileHeader extends StatelessWidget {
  const SocialProfileHeader({
    required this.displayName,
    required this.username,
    required this.role,
    required this.posts,
    required this.followers,
    required this.following,
    this.avatarUrl,
    this.bio,
    this.primaryActionLabel,
    this.secondaryActionLabel,
    this.onPrimaryAction,
    this.onSecondaryAction,
    super.key,
  });

  final String displayName;
  final String username;
  final String role;
  final int posts;
  final int followers;
  final int following;
  final String? avatarUrl;
  final String? bio;
  final String? primaryActionLabel;
  final String? secondaryActionLabel;
  final VoidCallback? onPrimaryAction;
  final VoidCallback? onSecondaryAction;

  @override
  Widget build(BuildContext context) {
    final hasAvatar = (avatarUrl ?? '').isNotEmpty;

    return Container(
      padding: const EdgeInsets.all(EventXSpacing.md),
      decoration: BoxDecoration(
        color: EventXColors.socialSurface,
        borderRadius: BorderRadius.circular(EventXRadius.lg),
        border: Border.all(color: EventXColors.socialStroke),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Container(
                padding: const EdgeInsets.all(2),
                decoration: const BoxDecoration(
                  shape: BoxShape.circle,
                  gradient: LinearGradient(
                    colors: [
                      EventXColors.socialAccent,
                      EventXColors.socialAccentAlt
                    ],
                    begin: Alignment.topLeft,
                    end: Alignment.bottomRight,
                  ),
                ),
                child: CircleAvatar(
                  radius: 40,
                  backgroundColor: EventXColors.socialSurfaceAlt,
                  backgroundImage: hasAvatar ? NetworkImage(avatarUrl!) : null,
                  child: hasAvatar
                      ? null
                      : Text(
                          displayName.trim().isEmpty
                              ? 'U'
                              : displayName.trim().characters.first,
                          style: const TextStyle(
                            color: EventXColors.socialText,
                            fontWeight: FontWeight.w700,
                            fontSize: 24,
                          ),
                        ),
                ),
              ),
              const SizedBox(width: EventXSpacing.md),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      displayName,
                      style: const TextStyle(
                        color: EventXColors.socialText,
                        fontWeight: FontWeight.w800,
                        fontSize: 22,
                      ),
                    ),
                    const SizedBox(height: 2),
                    Text(
                      username,
                      style: const TextStyle(
                        color: EventXColors.socialTextMuted,
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                    const SizedBox(height: EventXSpacing.xs),
                    Container(
                      padding: const EdgeInsets.symmetric(
                        horizontal: EventXSpacing.xs,
                        vertical: 4,
                      ),
                      decoration: BoxDecoration(
                        color: EventXColors.socialSurfaceAlt,
                        borderRadius: BorderRadius.circular(EventXRadius.pill),
                      ),
                      child: Text(
                        role,
                        style: const TextStyle(
                          color: EventXColors.socialTextMuted,
                          fontSize: 11,
                        ),
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
          if ((bio ?? '').isNotEmpty) ...[
            const SizedBox(height: EventXSpacing.sm),
            Text(
              bio!,
              style: const TextStyle(color: EventXColors.socialTextMuted),
            ),
          ],
          const SizedBox(height: EventXSpacing.md),
          ProfileStatsRow(
            posts: posts,
            followers: followers,
            following: following,
          ),
          if (primaryActionLabel != null || secondaryActionLabel != null) ...[
            const SizedBox(height: EventXSpacing.md),
            Row(
              children: [
                if (primaryActionLabel != null)
                  Expanded(
                    child: FilledButton(
                      onPressed: onPrimaryAction,
                      child: Text(primaryActionLabel!),
                    ),
                  ),
                if (primaryActionLabel != null && secondaryActionLabel != null)
                  const SizedBox(width: EventXSpacing.xs),
                if (secondaryActionLabel != null)
                  Expanded(
                    child: OutlinedButton(
                      onPressed: onSecondaryAction,
                      child: Text(secondaryActionLabel!),
                    ),
                  ),
              ],
            ),
          ],
        ],
      ),
    );
  }
}
