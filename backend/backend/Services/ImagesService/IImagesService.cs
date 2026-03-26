public interface IImagesService
{
    Task<OrderResponse> UploadLoadingPhotoAsync(int orderId, IFormFile file, CancellationToken cancellationToken = default);
    Task<OrderResponse> UploadDeliveryPhotoAsync(int orderId, IFormFile file, CancellationToken cancellationToken = default);
}
