public sealed class ImagesRepository(AppDbContext dbContext, SupabaseAppOptions options) : IImagesRepository
{
    public async Task<string> UploadAsync(byte[] data, string path, string contentType, CancellationToken cancellationToken = default)
    {
        var bucket = dbContext.Client.Storage.From(options.StorageBucket);

        await bucket.Upload(
            data,
            path,
            new Supabase.Storage.FileOptions
            {
                ContentType = contentType,
                Upsert = true,
                CacheControl = "3600"
            });

        cancellationToken.ThrowIfCancellationRequested();

        return bucket.GetPublicUrl(path);
    }
}
