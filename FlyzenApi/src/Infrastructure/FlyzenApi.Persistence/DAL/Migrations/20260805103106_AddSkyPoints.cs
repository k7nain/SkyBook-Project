using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlyzenApi.Persistence.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddSkyPoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SkyPointsBalance",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SkyPoints",
                table: "Flights",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "SkyPointsTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    RelatedBookingId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkyPointsTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SkyPointsTransactions_Bookings_RelatedBookingId",
                        column: x => x.RelatedBookingId,
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SkyPointsTransactions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SkyPointsTransactions_RelatedBookingId",
                table: "SkyPointsTransactions",
                column: "RelatedBookingId");

            migrationBuilder.CreateIndex(
                name: "IX_SkyPointsTransactions_UserId",
                table: "SkyPointsTransactions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SkyPointsTransactions");

            migrationBuilder.DropColumn(
                name: "SkyPointsBalance",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SkyPoints",
                table: "Flights");
        }
    }
}
