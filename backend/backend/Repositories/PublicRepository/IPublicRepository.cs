public interface IPublicRepository
{
	Task<Order?> TrackByInvoiceAndCustomerAsync(string invoiceNumber, string customerNumber, CancellationToken cancellationToken = default);
}
