public sealed class ImagesService(
    IImagesRepository imagesRepository,
    IOrdersService ordersService) : IImagesService
{
    public async Task<OrderResponse> UploadLoadingPhotoAsync(int orderId, IFormFile file, CancellationToken cancellationToken = default)
    {
        var photoUrl = await UploadAsync(orderId, file, "loading", cancellationToken);
        return await ordersService.AttachLoadingPhotoAsync(orderId, photoUrl, cancellationToken);
    }

    public async Task<OrderResponse> UploadDeliveryPhotoAsync(int orderId, IFormFile file, CancellationToken cancellationToken = default)
    {
        var photoUrl = await UploadAsync(orderId, file, "delivery", cancellationToken);
        return await ordersService.AttachDeliveryPhotoAsync(orderId, photoUrl, cancellationToken);
    }

    private async Task<string> UploadAsync(int orderId, IFormFile file, string type, CancellationToken cancellationToken)
    {
        if (file.Length == 0)
        {
            throw new ApiException("File is empty.", StatusCodes.Status400BadRequest);
        }

        if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            throw new ApiException("Only image files are allowed.", StatusCodes.Status400BadRequest);
        }

        await using var stream = file.OpenReadStream();
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory, cancellationToken);

        var extension = Path.GetExtension(file.FileName);
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var path = $"orders/{orderId}/{type}_{timestamp}{extension}";

        return await imagesRepository.UploadAsync(memory.ToArray(), path, file.ContentType, cancellationToken);
    }
}
