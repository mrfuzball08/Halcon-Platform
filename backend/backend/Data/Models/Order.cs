using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

[Table("orders")]
public class Order : BaseModel
{
	[PrimaryKey("id", false)]
	public int Id { get; set; }

	[Column("invoice_number")]
	public string InvoiceNumber { get; set; } = string.Empty;

	[Column("customer_number")]
	public string CustomerNumber { get; set; } = string.Empty;

	[Column("customer_name")]
	public string CustomerName { get; set; } = string.Empty;

	[Column("fiscal_data")]
	public string FiscalData { get; set; } = string.Empty;

	[Column("delivery_address")]
	public string DeliveryAddress { get; set; } = string.Empty;

	[Column("notes")]
	public string Notes { get; set; } = string.Empty;

	[Column("status")]
	public string Status { get; set; } = OrderStatus.ORDERED.ToString();

	[Column("created_at")]
	public DateTime CreatedAt { get; set; }

	[Column("updated_at")]
	public DateTime UpdatedAt { get; set; }

	[Column("is_deleted")]
	public bool IsDeleted { get; set; }

	[Column("loading_photo_url")]
	public string? LoadingPhotoUrl { get; set; }

	[Column("delivery_photo_url")]
	public string? DeliveryPhotoUrl { get; set; }
}
