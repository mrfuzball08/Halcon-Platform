public sealed class PublicOrderTrackResponse
{
	public string InvoiceNumber { get; set; } = string.Empty;
	public string CustomerName { get; set; } = string.Empty;
	public string Status { get; set; } = string.Empty;
	public string? DeliveryPhotoUrl { get; set; }
}
