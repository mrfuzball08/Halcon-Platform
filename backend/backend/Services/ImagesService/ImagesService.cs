public sealed class ImagesService(
    IImagesRepository imagesRepository,
    IOrdersService ordersService) : IImagesService
{
    private const long MaxUploadBytes = 10 * 1024 * 1024;

    public async Task<OrderResponse> UploadLoadingPhotoAsync(int orderId, IFormFile file, CancellationToken cancellationToken = default)
    {
        await EnsureOrderIsInRouteAsync(orderId, cancellationToken);
        var photoUrl = await UploadAsync(orderId, file, "loading", cancellationToken);
        return await ordersService.AttachLoadingPhotoAsync(orderId, photoUrl, cancellationToken);
    }

    public async Task<OrderResponse> UploadDeliveryPhotoAsync(int orderId, IFormFile file, CancellationToken cancellationToken = default)
    {
        await EnsureOrderIsInRouteAsync(orderId, cancellationToken);
        var photoUrl = await UploadAsync(orderId, file, "delivery", cancellationToken);
        return await ordersService.AttachDeliveryPhotoAsync(orderId, photoUrl, cancellationToken);
    }

    private async Task EnsureOrderIsInRouteAsync(int orderId, CancellationToken cancellationToken)
    {
        var order = await ordersService.GetAsync(orderId, cancellationToken);
        if (!string.Equals(order.Status, nameof(OrderStatus.IN_ROUTE), StringComparison.Ordinal))
        {
            throw new ApiException(
                "Photos can only be uploaded when the order status is IN_ROUTE.",
                StatusCodes.Status400BadRequest);
        }
    }

    private async Task<string> UploadAsync(int orderId, IFormFile file, string type, CancellationToken cancellationToken)
    {
        if (file.Length == 0)
        {
            throw new ApiException("File is empty.", StatusCodes.Status400BadRequest);
        }

        if (file.Length > MaxUploadBytes)
        {
            throw new ApiException("File exceeds the 10 MB limit.", StatusCodes.Status413PayloadTooLarge);
        }

        if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            throw new ApiException("Only image files are allowed.", StatusCodes.Status400BadRequest);
        }

        await using var stream = file.OpenReadStream();
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory, cancellationToken);

        var extension = Path.GetExtension(file.FileName);
        var uniqueSuffix = $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{Guid.NewGuid():N}";
        var path = $"orders/{orderId}/{type}_{uniqueSuffix}{extension}";

        return await imagesRepository.UploadAsync(memory.ToArray(), path, file.ContentType, cancellationToken);
    }
}
