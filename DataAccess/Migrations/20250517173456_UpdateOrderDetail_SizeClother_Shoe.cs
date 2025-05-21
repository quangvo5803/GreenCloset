using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrderDetail_SizeClother_Shoe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SizeClother",
                table: "OrderDetails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SizeShoe",
                table: "OrderDetails",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SizeClother",
                table: "OrderDetails");

            migrationBuilder.DropColumn(
                name: "SizeShoe",
                table: "OrderDetails");
        }
    }
}
