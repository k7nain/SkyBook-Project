using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlyzenApi.Persistence.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddTripPlaceImageDisplayOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "TripPlaceImages",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "TripPlaceImages");
        }
    }
}
