import 'package:flutter/material.dart';
import 'package:projeto_eventx_flutter/shared/widgets/social/profile_stat_item.dart';

class ProfileStats extends StatelessWidget {
  const ProfileStats({
    required this.posts,
    required this.followers,
    required this.following,
    super.key,
  });

  final int posts;
  final int followers;
  final int following;

  @override
  Widget build(BuildContext context) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceEvenly,
      children: [
        ProfileStatItem(label: 'Posts', value: posts.toString()),
        ProfileStatItem(label: 'Seguidores', value: followers.toString()),
        ProfileStatItem(label: 'Seguindo', value: following.toString()),
      ],
    );
  }
}
