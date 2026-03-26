using Supabase.Postgrest.Interfaces;
using Supabase.Postgrest;

public sealed class OrdersRepository(AppDbContext dbContext) : IOrdersRepository
{
    public async Task<List<Order>> ListAsync(
        string? invoice,
        string? customer,
        string? date,
        string? status,
        bool deletedOnly,
        CancellationToken cancellationToken = default)
    {
        IPostgrestTable<Order> query = dbContext.Client
            .From<Order>()
            .Order("created_at", Constants.Ordering.Descending)
            .Filter("is_deleted", Constants.Operator.Equals, deletedOnly);

        if (!string.IsNullOrWhiteSpace(invoice))
        {
            query = query.Filter("invoice_number", Constants.Operator.ILike, $"%{invoice}%");
        }

        if (!string.IsNullOrWhiteSpace(customer))
        {
            query = query.Filter("customer_number", Constants.Operator.ILike, $"%{customer}%");
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Filter("status", Constants.Operator.Equals, status);
        }

        if (!string.IsNullOrWhiteSpace(date))
        {
            query = query.Filter("created_at", Constants.Operator.GreaterThanOrEqual, $"{date}T00:00:00");
        }

        var response = await query.Get(cancellationToken);
        return response.Models;
    }

    public async Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await dbContext.Client
            .From<Order>()
            .Filter("id", Constants.Operator.Equals, id)
            .Limit(1)
            .Get(cancellationToken);

        return response.Models.FirstOrDefault();
    }

    public async Task<Order?> GetByInvoiceAsync(string invoiceNumber, CancellationToken cancellationToken = default)
    {
        var response = await dbContext.Client
            .From<Order>()
            .Filter("invoice_number", Constants.Operator.Equals, invoiceNumber)
            .Limit(1)
            .Get(cancellationToken);

        return response.Models.FirstOrDefault();
    }

    public async Task<Order> CreateAsync(Order order, CancellationToken cancellationToken = default)
    {
        var response = await dbContext.Client
            .From<Order>()
            .Insert(order, cancellationToken: cancellationToken);

        return response.Models.First();
    }

    public async Task<Order?> UpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        var response = await dbContext.Client
            .From<Order>()
            .Filter("id", Constants.Operator.Equals, order.Id)
            .Update(order, cancellationToken: cancellationToken);

        return response.Models.FirstOrDefault();
    }
}
