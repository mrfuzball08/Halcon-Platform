public interface IImagesRepository
{
	Task<string> UploadAsync(byte[] data, string path, string contentType, CancellationToken cancellationToken = default);
}
