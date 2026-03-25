public sealed class OrderResponse
{
	public int Id { get; set; }
	public string InvoiceNumber { get; set; } = string.Empty;
	public string CustomerNumber { get; set; } = string.Empty;
	public string CustomerName { get; set; } = string.Empty;
	public string FiscalData { get; set; } = string.Empty;
	public string DeliveryAddress { get; set; } = string.Empty;
	public string Notes { get; set; } = string.Empty;
	public string Status { get; set; } = string.Empty;
	public DateTime CreatedAt { get; set; }
	public DateTime UpdatedAt { get; set; }
	public bool IsDeleted { get; set; }
	public string? LoadingPhotoUrl { get; set; }
	public string? DeliveryPhotoUrl { get; set; }
}
