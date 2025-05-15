using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSizeToCart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Size",
                table: "Carts");

            migrationBuilder.AddColumn<int>(
                name: "SizeClother",
                table: "Carts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SizeShoe",
                table: "Carts",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SizeClother",
                table: "Carts");

            migrationBuilder.DropColumn(
                name: "SizeShoe",
                table: "Carts");

            migrationBuilder.AddColumn<string>(
                name: "Size",
                table: "Carts",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
