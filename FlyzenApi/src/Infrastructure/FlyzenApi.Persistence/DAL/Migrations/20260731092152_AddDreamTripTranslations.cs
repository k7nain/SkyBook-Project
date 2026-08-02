using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlyzenApi.Persistence.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddDreamTripTranslations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "TripPlaces",
                newName: "NameEn");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "TripPlaces",
                newName: "DescriptionRu");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "TripCountries",
                newName: "NameEn");

            migrationBuilder.RenameColumn(
                name: "ShortDescription",
                table: "TripCities",
                newName: "ShortDescriptionRu");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "TripCities",
                newName: "NameEn");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionAz",
                table: "TripPlaces",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionEn",
                table: "TripPlaces",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAz",
                table: "TripPlaces",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NameRu",
                table: "TripPlaces",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAz",
                table: "TripCountries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NameRu",
                table: "TripCountries",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAz",
                table: "TripCities",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NameRu",
                table: "TripCities",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortDescriptionAz",
                table: "TripCities",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortDescriptionEn",
                table: "TripCities",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DescriptionAz",
                table: "TripPlaces");

            migrationBuilder.DropColumn(
                name: "DescriptionEn",
                table: "TripPlaces");

            migrationBuilder.DropColumn(
                name: "NameAz",
                table: "TripPlaces");

            migrationBuilder.DropColumn(
                name: "NameRu",
                table: "TripPlaces");

            migrationBuilder.DropColumn(
                name: "NameAz",
                table: "TripCountries");

            migrationBuilder.DropColumn(
                name: "NameRu",
                table: "TripCountries");

            migrationBuilder.DropColumn(
                name: "NameAz",
                table: "TripCities");

            migrationBuilder.DropColumn(
                name: "NameRu",
                table: "TripCities");

            migrationBuilder.DropColumn(
                name: "ShortDescriptionAz",
                table: "TripCities");

            migrationBuilder.DropColumn(
                name: "ShortDescriptionEn",
                table: "TripCities");

            migrationBuilder.RenameColumn(
                name: "NameEn",
                table: "TripPlaces",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "DescriptionRu",
                table: "TripPlaces",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "NameEn",
                table: "TripCountries",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "ShortDescriptionRu",
                table: "TripCities",
                newName: "ShortDescription");

            migrationBuilder.RenameColumn(
                name: "NameEn",
                table: "TripCities",
                newName: "Name");
        }
    }
}
