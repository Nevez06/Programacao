using EventX.Api.DTOs.Orders;

namespace EventX.Api.Services;

public interface IOrderService
{
    Task<OrderDto?> CreateOrderAsync(
        Guid organizerId,
        CreateOrderRequestDto request,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<OrderDto>> GetOrdersAsync(
        Guid organizerId,
        OrderQueryDto query,
        CancellationToken cancellationToken);

    Task<OrderDto?> GetOrderByIdAsync(Guid organizerId, string orderId, CancellationToken cancellationToken);

    Task<OrderDto?> UpdateOrderStatusAsync(
        Guid organizerId,
        string orderId,
        UpdateOrderStatusRequestDto request,
        CancellationToken cancellationToken);
}
