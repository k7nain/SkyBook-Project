using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlyzenApi.Persistence.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrencyFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "MealOptions",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "AZN");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Flights",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "AZN");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Bookings",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "AZN");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "BaggageOptions",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "AZN");

            // Existing users had "USD" written explicitly at registration time (the old
            // C# default) - AZN is now the app-wide default, so bring them in line too.
            migrationBuilder.Sql("UPDATE \"Users\" SET \"CurrencyPreference\" = 'AZN' WHERE \"CurrencyPreference\" = 'USD';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Currency",
                table: "MealOptions");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "BaggageOptions");
        }
    }
}
