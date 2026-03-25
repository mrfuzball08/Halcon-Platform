using Microsoft.EntityFrameworkCore;

namespace Backend.Migrations;

public sealed class BackendMigrationsDbContext(DbContextOptions<BackendMigrationsDbContext> options) : DbContext(options)
{
    public DbSet<ProfileEntity> Profiles => Set<ProfileEntity>();
    public DbSet<OrderEntity> Orders => Set<OrderEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");

        modelBuilder.Entity<ProfileEntity>(entity =>
        {
            entity.ToTable("profiles");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.AuthUserId).HasColumnName("auth_user_id");
            entity.Property(x => x.Username).HasColumnName("username");
            entity.Property(x => x.Email).HasColumnName("email");
            entity.Property(x => x.Role).HasColumnName("role");

            entity.HasIndex(x => x.AuthUserId).IsUnique();
            entity.HasIndex(x => x.Username).IsUnique();
            entity.HasIndex(x => x.Email).IsUnique();

            entity.ToTable(table =>
            {
                table.HasCheckConstraint("ck_profiles_role", "role in ('ADMIN', 'SALES', 'PURCHASING', 'WAREHOUSE', 'ROUTE')");
            });
        });

        modelBuilder.Entity<OrderEntity>(entity =>
        {
            entity.ToTable("orders");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.InvoiceNumber).HasColumnName("invoice_number");
            entity.Property(x => x.CustomerNumber).HasColumnName("customer_number");
            entity.Property(x => x.CustomerName).HasColumnName("customer_name");
            entity.Property(x => x.FiscalData).HasColumnName("fiscal_data");
            entity.Property(x => x.DeliveryAddress).HasColumnName("delivery_address");
            entity.Property(x => x.Notes).HasColumnName("notes");
            entity.Property(x => x.Status).HasColumnName("status");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.Property(x => x.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
            entity.Property(x => x.LoadingPhotoUrl).HasColumnName("loading_photo_url");
            entity.Property(x => x.DeliveryPhotoUrl).HasColumnName("delivery_photo_url");

            entity.HasIndex(x => x.InvoiceNumber).IsUnique();
            entity.HasIndex(x => x.CustomerNumber);
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.CreatedAt);
            entity.HasIndex(x => x.IsDeleted);

            entity.ToTable(table =>
            {
                table.HasCheckConstraint("ck_orders_status", "status in ('ORDERED', 'IN_PROCESS', 'IN_ROUTE', 'DELIVERED')");
            });
        });
    }
}
