using Xunit;

public sealed class OrdersServiceTests
{
    [Fact]
    public async Task CreateAsync_WhenInvoiceOrCustomerMissing_ThrowsBadRequest()
    {
        var repository = new FakeOrdersRepository();
        var service = new OrdersService(repository);

        var request = new OrderCreateRequest
        {
            InvoiceNumber = "",
            CustomerNumber = ""
        };

        var exception = await Assert.ThrowsAsync<ApiException>(() => service.CreateAsync(request));

        Assert.Equal(400, exception.StatusCode);
    }

    [Fact]
    public async Task CreateAsync_WhenInvoiceAlreadyExists_ThrowsConflict()
    {
        var repository = new FakeOrdersRepository
        {
            GetByInvoiceResult = new Order { Id = 2, InvoiceNumber = "INV-001" }
        };

        var service = new OrdersService(repository);

        var request = new OrderCreateRequest
        {
            InvoiceNumber = "INV-001",
            CustomerNumber = "CUS-001"
        };

        var exception = await Assert.ThrowsAsync<ApiException>(() => service.CreateAsync(request));

        Assert.Equal(409, exception.StatusCode);
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenWarehouseMovesOrderedToInProcess_UpdatesOrder()
    {
        var repository = new FakeOrdersRepository
        {
            GetByIdResult = new Order
            {
                Id = 10,
                InvoiceNumber = "INV-010",
                CustomerNumber = "CUS-010",
                Status = nameof(OrderStatus.ORDERED),
                DeliveryPhotoUrl = null,
                IsDeleted = false
            }
        };

        var service = new OrdersService(repository);

        var result = await service.UpdateStatusAsync(
            10,
            nameof(OrderStatus.IN_PROCESS),
            nameof(UserRole.WAREHOUSE));

        Assert.Equal(nameof(OrderStatus.IN_PROCESS), result.Status);
        Assert.NotNull(repository.UpdatedOrder);
        Assert.Equal(nameof(OrderStatus.IN_PROCESS), repository.UpdatedOrder!.Status);
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenRoleNotAllowed_ThrowsForbidden()
    {
        var repository = new FakeOrdersRepository
        {
            GetByIdResult = new Order
            {
                Id = 11,
                Status = nameof(OrderStatus.ORDERED),
                IsDeleted = false
            }
        };

        var service = new OrdersService(repository);

        var exception = await Assert.ThrowsAsync<ApiException>(() =>
            service.UpdateStatusAsync(11, nameof(OrderStatus.IN_PROCESS), nameof(UserRole.ROUTE)));

        Assert.Equal(403, exception.StatusCode);
        Assert.Null(repository.UpdatedOrder);
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenSettingDeliveredWithoutDeliveryPhoto_ThrowsBadRequest()
    {
        var repository = new FakeOrdersRepository
        {
            GetByIdResult = new Order
            {
                Id = 12,
                Status = nameof(OrderStatus.IN_ROUTE),
                DeliveryPhotoUrl = null,
                IsDeleted = false
            }
        };

        var service = new OrdersService(repository);

        var exception = await Assert.ThrowsAsync<ApiException>(() =>
            service.UpdateStatusAsync(12, nameof(OrderStatus.DELIVERED), nameof(UserRole.ROUTE)));

        Assert.Equal(400, exception.StatusCode);
        Assert.Null(repository.UpdatedOrder);
    }

    [Fact]
    public async Task ListAsync_WhenDateFormatIsInvalid_ThrowsBadRequest()
    {
        var repository = new FakeOrdersRepository();
        var service = new OrdersService(repository);

        var exception = await Assert.ThrowsAsync<ApiException>(() =>
            service.ListAsync(null, null, "03-26-2026", null, false));

        Assert.Equal(400, exception.StatusCode);
    }

    private sealed class FakeOrdersRepository : IOrdersRepository
    {
        public Order? GetByIdResult { get; set; }
        public Order? GetByInvoiceResult { get; set; }
        public List<Order> ListResult { get; set; } = [];
        public Order? UpdatedOrder { get; private set; }

        public Task<Order> CreateAsync(Order order, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(order);
        }

        public Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(GetByIdResult);
        }

        public Task<Order?> GetByInvoiceAsync(string invoiceNumber, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(GetByInvoiceResult);
        }

        public Task<List<Order>> ListAsync(string? invoice, string? customer, string? date, string? status, bool deletedOnly, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ListResult);
        }

        public Task<Order?> UpdateAsync(Order order, CancellationToken cancellationToken = default)
        {
            UpdatedOrder = order;
            return Task.FromResult<Order?>(order);
        }
    }
}
