using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlyzenApi.Persistence.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddBaggageDisplayOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "BaggageOptions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Backfill already-seeded rows so 10kg keeps its 2nd-row position instead of
            // everyone landing on the same default 0 (fresh installs get this straight
            // from DbInitializer's seed list, this only patches pre-existing data).
            migrationBuilder.Sql("UPDATE \"BaggageOptions\" SET \"DisplayOrder\" = 0 WHERE \"WeightKg\" = 0;");
            migrationBuilder.Sql("UPDATE \"BaggageOptions\" SET \"DisplayOrder\" = 1 WHERE \"WeightKg\" = 10;");
            migrationBuilder.Sql("UPDATE \"BaggageOptions\" SET \"DisplayOrder\" = 2 WHERE \"WeightKg\" = 20;");
            migrationBuilder.Sql("UPDATE \"BaggageOptions\" SET \"DisplayOrder\" = 3 WHERE \"WeightKg\" = 30;");
            migrationBuilder.Sql("UPDATE \"BaggageOptions\" SET \"DisplayOrder\" = 4 WHERE \"WeightKg\" = 40;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "BaggageOptions");
        }
    }
}
