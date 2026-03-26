public interface IOrdersRepository
{
    Task<List<Order>> ListAsync(string? invoice, string? customer, string? date, string? status, bool deletedOnly, CancellationToken cancellationToken = default);
    Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Order?> GetByInvoiceAsync(string invoiceNumber, CancellationToken cancellationToken = default);
    Task<Order> CreateAsync(Order order, CancellationToken cancellationToken = default);
    Task<Order?> UpdateAsync(Order order, CancellationToken cancellationToken = default);
}
