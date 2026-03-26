using Xunit;

public sealed class PublicServiceTests
{
    [Fact]
    public async Task TrackAsync_WhenOrderNotDelivered_HidesDeliveryPhoto()
    {
        var repository = new FakePublicRepository
        {
            TrackedOrder = new Order
            {
                InvoiceNumber = "INV-100",
                CustomerNumber = "CUS-100",
                CustomerName = "ACME",
                Status = nameof(OrderStatus.IN_ROUTE),
                DeliveryPhotoUrl = "https://cdn.example.com/photo.jpg"
            }
        };

        var service = new PublicService(repository);

        var result = await service.TrackAsync("INV-100", "CUS-100");

        Assert.Equal(nameof(OrderStatus.IN_ROUTE), result.Status);
        Assert.Null(result.DeliveryPhotoUrl);
    }

    [Fact]
    public async Task TrackAsync_WhenOrderDelivered_ShowsDeliveryPhoto()
    {
        var repository = new FakePublicRepository
        {
            TrackedOrder = new Order
            {
                InvoiceNumber = "INV-101",
                CustomerNumber = "CUS-101",
                CustomerName = "ACME",
                Status = nameof(OrderStatus.DELIVERED),
                DeliveryPhotoUrl = "https://cdn.example.com/delivery.jpg"
            }
        };

        var service = new PublicService(repository);

        var result = await service.TrackAsync("INV-101", "CUS-101");

        Assert.Equal(nameof(OrderStatus.DELIVERED), result.Status);
        Assert.Equal("https://cdn.example.com/delivery.jpg", result.DeliveryPhotoUrl);
    }

    [Fact]
    public async Task TrackAsync_WhenInvoiceOrCustomerMissing_ThrowsBadRequest()
    {
        var repository = new FakePublicRepository();
        var service = new PublicService(repository);

        var exception = await Assert.ThrowsAsync<ApiException>(() => service.TrackAsync("", "CUS-200"));

        Assert.Equal(400, exception.StatusCode);
    }

    private sealed class FakePublicRepository : IPublicRepository
    {
        public Order? TrackedOrder { get; set; }

        public Task<Order?> TrackByInvoiceAndCustomerAsync(string invoiceNumber, string customerNumber, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(TrackedOrder);
        }
    }
}
