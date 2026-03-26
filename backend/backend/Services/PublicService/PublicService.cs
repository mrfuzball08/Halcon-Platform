public sealed class PublicService(IPublicRepository publicRepository) : IPublicService
{
    public async Task<PublicOrderTrackResponse> TrackAsync(string invoice, string customer, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(invoice) || string.IsNullOrWhiteSpace(customer))
        {
            throw new ApiException("Invoice and customer are required.", StatusCodes.Status400BadRequest);
        }

        var order = await publicRepository.TrackByInvoiceAndCustomerAsync(invoice, customer, cancellationToken);
        if (order is null)
        {
            throw new ApiException("Order not found.", StatusCodes.Status404NotFound);
        }

        return new PublicOrderTrackResponse
        {
            InvoiceNumber = order.InvoiceNumber,
            CustomerName = order.CustomerName,
            Status = order.Status,
            DeliveryPhotoUrl = string.Equals(order.Status, OrderStatus.DELIVERED.ToString(), StringComparison.Ordinal)
                ? order.DeliveryPhotoUrl
                : null
        };
    }
}
