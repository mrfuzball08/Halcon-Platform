public interface IOrdersService
{
    Task<List<OrderResponse>> ListAsync(string? invoice, string? customer, string? date, string? status, bool deletedOnly, CancellationToken cancellationToken = default);
    Task<OrderResponse> GetAsync(int id, CancellationToken cancellationToken = default);
    Task<OrderResponse> CreateAsync(OrderCreateRequest request, CancellationToken cancellationToken = default);
    Task<OrderResponse> UpdateAsync(int id, OrderUpdateRequest request, CancellationToken cancellationToken = default);
    Task<OrderResponse> UpdateStatusAsync(int id, string newStatus, string actorRole, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<OrderResponse> RestoreAsync(int id, CancellationToken cancellationToken = default);
    Task<OrderResponse> AttachLoadingPhotoAsync(int id, string photoUrl, CancellationToken cancellationToken = default);
    Task<OrderResponse> AttachDeliveryPhotoAsync(int id, string photoUrl, CancellationToken cancellationToken = default);
}
