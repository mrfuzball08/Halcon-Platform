using Supabase.Postgrest;

public sealed class PublicRepository(AppDbContext dbContext) : IPublicRepository
{
    public async Task<Order?> TrackByInvoiceAndCustomerAsync(string invoiceNumber, string customerNumber, CancellationToken cancellationToken = default)
    {
        var response = await dbContext.Client
            .From<Order>()
            .Filter("invoice_number", Constants.Operator.Equals, invoiceNumber)
            .Filter("customer_number", Constants.Operator.Equals, customerNumber)
            .Filter("is_deleted", Constants.Operator.Equals, false)
            .Limit(1)
            .Get(cancellationToken);

        return response.Models.FirstOrDefault();
    }
}
