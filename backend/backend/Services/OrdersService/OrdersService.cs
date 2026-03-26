using System.Globalization;

public sealed class OrdersService(IOrdersRepository ordersRepository) : IOrdersService
{
    public async Task<List<OrderResponse>> ListAsync(string? invoice, string? customer, string? date, string? status, bool deletedOnly, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(status) && !DomainValidation.IsValidStatus(status))
        {
            throw new ApiException("Status filter is invalid.", StatusCodes.Status400BadRequest);
        }

        string? normalizedDate = null;
        if (!string.IsNullOrWhiteSpace(date))
        {
            if (!DateOnly.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
            {
                throw new ApiException("Date filter must use yyyy-MM-dd format.", StatusCodes.Status400BadRequest);
            }

            normalizedDate = parsedDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        var orders = await ordersRepository.ListAsync(invoice, customer, normalizedDate, status, deletedOnly, cancellationToken);
        return orders.Select(x => x.ToResponse()).ToList();
    }

    public async Task<OrderResponse> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await ordersRepository.GetByIdAsync(id, cancellationToken);
        if (order is null)
        {
            throw new ApiException("Order not found.", StatusCodes.Status404NotFound);
        }

        return order.ToResponse();
    }

    public async Task<OrderResponse> CreateAsync(OrderCreateRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.InvoiceNumber) || string.IsNullOrWhiteSpace(request.CustomerNumber))
        {
            throw new ApiException("Invoice number and customer number are required.", StatusCodes.Status400BadRequest);
        }

        var duplicate = await ordersRepository.GetByInvoiceAsync(request.InvoiceNumber, cancellationToken);
        if (duplicate is not null)
        {
            throw new ApiException("Invoice number already exists.", StatusCodes.Status409Conflict);
        }

        var now = DateTime.UtcNow;
        var order = new Order
        {
            InvoiceNumber = request.InvoiceNumber,
            CustomerNumber = request.CustomerNumber,
            CustomerName = request.CustomerName,
            FiscalData = request.FiscalData,
            DeliveryAddress = request.DeliveryAddress,
            Notes = request.Notes,
            Status = OrderStatus.ORDERED.ToString(),
            CreatedAt = now,
            UpdatedAt = now,
            IsDeleted = false
        };

        var created = await ordersRepository.CreateAsync(order, cancellationToken);
        return created.ToResponse();
    }

    public async Task<OrderResponse> UpdateAsync(int id, OrderUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var order = await ordersRepository.GetByIdAsync(id, cancellationToken);
        if (order is null)
        {
            throw new ApiException("Order not found.", StatusCodes.Status404NotFound);
        }

        if (order.IsDeleted)
        {
            throw new ApiException("Cannot update a deleted order.", StatusCodes.Status400BadRequest);
        }

        if (!string.IsNullOrWhiteSpace(request.InvoiceNumber) && !string.Equals(order.InvoiceNumber, request.InvoiceNumber, StringComparison.Ordinal))
        {
            var duplicate = await ordersRepository.GetByInvoiceAsync(request.InvoiceNumber, cancellationToken);
            if (duplicate is not null)
            {
                throw new ApiException("Invoice number already exists.", StatusCodes.Status409Conflict);
            }

            order.InvoiceNumber = request.InvoiceNumber;
        }

        order.CustomerNumber = request.CustomerNumber ?? order.CustomerNumber;
        order.CustomerName = request.CustomerName ?? order.CustomerName;
        order.FiscalData = request.FiscalData ?? order.FiscalData;
        order.DeliveryAddress = request.DeliveryAddress ?? order.DeliveryAddress;
        order.Notes = request.Notes ?? order.Notes;
        order.UpdatedAt = DateTime.UtcNow;

        var updated = await ordersRepository.UpdateAsync(order, cancellationToken);
        if (updated is null)
        {
            throw new ApiException("Order update failed.", StatusCodes.Status500InternalServerError);
        }

        return updated.ToResponse();
    }

    public async Task<OrderResponse> UpdateStatusAsync(int id, string newStatus, string actorRole, CancellationToken cancellationToken = default)
    {
        if (!DomainValidation.IsValidStatus(newStatus))
        {
            throw new ApiException("Status is invalid.", StatusCodes.Status400BadRequest);
        }

        var order = await ordersRepository.GetByIdAsync(id, cancellationToken);
        if (order is null)
        {
            throw new ApiException("Order not found.", StatusCodes.Status404NotFound);
        }

        if (order.IsDeleted)
        {
            throw new ApiException("Cannot change status of a deleted order.", StatusCodes.Status400BadRequest);
        }

        EnforceTransition(order, newStatus, actorRole);

        if (string.Equals(newStatus, OrderStatus.DELIVERED.ToString(), StringComparison.Ordinal) && string.IsNullOrWhiteSpace(order.DeliveryPhotoUrl))
        {
            throw new ApiException("Delivery photo is required before setting DELIVERED.", StatusCodes.Status400BadRequest);
        }

        order.Status = newStatus;
        order.UpdatedAt = DateTime.UtcNow;

        var updated = await ordersRepository.UpdateAsync(order, cancellationToken);
        if (updated is null)
        {
            throw new ApiException("Order status update failed.", StatusCodes.Status500InternalServerError);
        }

        return updated.ToResponse();
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await ordersRepository.GetByIdAsync(id, cancellationToken);
        if (order is null)
        {
            throw new ApiException("Order not found.", StatusCodes.Status404NotFound);
        }

        order.IsDeleted = true;
        order.UpdatedAt = DateTime.UtcNow;

        var updated = await ordersRepository.UpdateAsync(order, cancellationToken);
        if (updated is null)
        {
            throw new ApiException("Order deletion failed.", StatusCodes.Status500InternalServerError);
        }
    }

    public async Task<OrderResponse> RestoreAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await ordersRepository.GetByIdAsync(id, cancellationToken);
        if (order is null)
        {
            throw new ApiException("Order not found.", StatusCodes.Status404NotFound);
        }

        order.IsDeleted = false;
        order.UpdatedAt = DateTime.UtcNow;

        var updated = await ordersRepository.UpdateAsync(order, cancellationToken);
        if (updated is null)
        {
            throw new ApiException("Order restore failed.", StatusCodes.Status500InternalServerError);
        }

        return updated.ToResponse();
    }

    public async Task<OrderResponse> AttachLoadingPhotoAsync(int id, string photoUrl, CancellationToken cancellationToken = default)
    {
        var order = await ordersRepository.GetByIdAsync(id, cancellationToken);
        if (order is null)
        {
            throw new ApiException("Order not found.", StatusCodes.Status404NotFound);
        }

        order.LoadingPhotoUrl = photoUrl;
        order.UpdatedAt = DateTime.UtcNow;

        var updated = await ordersRepository.UpdateAsync(order, cancellationToken);
        if (updated is null)
        {
            throw new ApiException("Failed to attach loading photo.", StatusCodes.Status500InternalServerError);
        }

        return updated.ToResponse();
    }

    public async Task<OrderResponse> AttachDeliveryPhotoAsync(int id, string photoUrl, CancellationToken cancellationToken = default)
    {
        var order = await ordersRepository.GetByIdAsync(id, cancellationToken);
        if (order is null)
        {
            throw new ApiException("Order not found.", StatusCodes.Status404NotFound);
        }

        order.DeliveryPhotoUrl = photoUrl;
        order.UpdatedAt = DateTime.UtcNow;

        var updated = await ordersRepository.UpdateAsync(order, cancellationToken);
        if (updated is null)
        {
            throw new ApiException("Failed to attach delivery photo.", StatusCodes.Status500InternalServerError);
        }

        return updated.ToResponse();
    }

    private static void EnforceTransition(Order order, string newStatus, string actorRole)
    {
        var allowed = (order.Status, newStatus) switch
        {
            (nameof(OrderStatus.ORDERED), nameof(OrderStatus.IN_PROCESS)) => actorRole is nameof(UserRole.WAREHOUSE) or nameof(UserRole.ADMIN),
            (nameof(OrderStatus.IN_PROCESS), nameof(OrderStatus.IN_ROUTE)) => actorRole is nameof(UserRole.WAREHOUSE) or nameof(UserRole.ADMIN),
            (nameof(OrderStatus.IN_ROUTE), nameof(OrderStatus.DELIVERED)) => actorRole is nameof(UserRole.ROUTE) or nameof(UserRole.ADMIN),
            _ => false
        };

        if (!allowed)
        {
            throw new ApiException("Status transition is not allowed.", StatusCodes.Status403Forbidden);
        }
    }
}
