class AsyncState<T> {
  const AsyncState._({
    this.data,
    this.error,
    required this.loading,
  });

  const AsyncState.idle() : this._(loading: false);

  const AsyncState.loading([T? previous])
      : this._(
          data: previous,
          loading: true,
        );

  const AsyncState.success(T value)
      : this._(
          data: value,
          loading: false,
        );

  const AsyncState.failure(String message, [T? previous])
      : this._(
          data: previous,
          error: message,
          loading: false,
        );

  final T? data;
  final String? error;
  final bool loading;

  bool get hasError => error != null && error!.isNotEmpty;
  bool get hasData => data != null;
}
