using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace backend.Migrations.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "orders",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    invoice_number = table.Column<string>(type: "text", nullable: false),
                    customer_number = table.Column<string>(type: "text", nullable: false),
                    customer_name = table.Column<string>(type: "text", nullable: false),
                    fiscal_data = table.Column<string>(type: "text", nullable: false),
                    delivery_address = table.Column<string>(type: "text", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    loading_photo_url = table.Column<string>(type: "text", nullable: true),
                    delivery_photo_url = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orders", x => x.id);
                    table.CheckConstraint("ck_orders_status", "status in ('ORDERED', 'IN_PROCESS', 'IN_ROUTE', 'DELIVERED')");
                });

            migrationBuilder.CreateTable(
                name: "profiles",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    auth_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_profiles", x => x.id);
                    table.CheckConstraint("ck_profiles_role", "role in ('ADMIN', 'SALES', 'PURCHASING', 'WAREHOUSE', 'ROUTE')");
                });

            migrationBuilder.CreateIndex(
                name: "IX_orders_created_at",
                schema: "public",
                table: "orders",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_orders_customer_number",
                schema: "public",
                table: "orders",
                column: "customer_number");

            migrationBuilder.CreateIndex(
                name: "IX_orders_invoice_number",
                schema: "public",
                table: "orders",
                column: "invoice_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_orders_is_deleted",
                schema: "public",
                table: "orders",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "IX_orders_status",
                schema: "public",
                table: "orders",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_profiles_auth_user_id",
                schema: "public",
                table: "profiles",
                column: "auth_user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_profiles_email",
                schema: "public",
                table: "profiles",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_profiles_username",
                schema: "public",
                table: "profiles",
                column: "username",
                unique: true);

            migrationBuilder.Sql(
                """
                ALTER TABLE public.profiles
                ADD CONSTRAINT fk_profiles_auth_users
                FOREIGN KEY (auth_user_id)
                REFERENCES auth.users(id)
                ON DELETE CASCADE;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE public.profiles
                DROP CONSTRAINT IF EXISTS fk_profiles_auth_users;
                """);

            migrationBuilder.DropTable(
                name: "orders",
                schema: "public");

            migrationBuilder.DropTable(
                name: "profiles",
                schema: "public");
        }
    }
}
