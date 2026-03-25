public interface IPublicService
{
	Task<PublicOrderTrackResponse> TrackAsync(string invoice, string customer, CancellationToken cancellationToken = default);
}
