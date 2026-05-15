class UpdateInviteModel {
  const UpdateInviteModel({
    this.status,
    this.checkInRealizado,
    this.templateId,
  });

  final String? status;
  final bool? checkInRealizado;
  final int? templateId;

  Map<String, dynamic> toJson() {
    return {
      'status': status,
      'checkInRealizado': checkInRealizado,
      'templateId': templateId,
    }..removeWhere((key, value) => value == null);
  }
}
