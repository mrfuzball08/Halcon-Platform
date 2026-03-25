public static class MappingExtensions
{
    public static UserResponse ToResponse(this User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            AuthUserId = user.AuthUserId,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role
        };
    }

    public static OrderResponse ToResponse(this Order order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            InvoiceNumber = order.InvoiceNumber,
            CustomerNumber = order.CustomerNumber,
            CustomerName = order.CustomerName,
            FiscalData = order.FiscalData,
            DeliveryAddress = order.DeliveryAddress,
            Notes = order.Notes,
            Status = order.Status,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            IsDeleted = order.IsDeleted,
            LoadingPhotoUrl = order.LoadingPhotoUrl,
            DeliveryPhotoUrl = order.DeliveryPhotoUrl
        };
    }
}
