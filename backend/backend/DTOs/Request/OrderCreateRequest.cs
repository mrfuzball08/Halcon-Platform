public sealed class OrderCreateRequest
{
	public string InvoiceNumber { get; set; } = string.Empty;
	public string CustomerNumber { get; set; } = string.Empty;
	public string CustomerName { get; set; } = string.Empty;
	public string FiscalData { get; set; } = string.Empty;
	public string DeliveryAddress { get; set; } = string.Empty;
	public string Notes { get; set; } = string.Empty;
}
