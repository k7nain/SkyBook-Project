using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlyzenApi.Persistence.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddCityTimeZoneId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TimeZoneId",
                table: "Cities",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "UTC");

            // Backfill real IANA zones for cities inserted by DbInitializer before
            // this column existed (all default to "UTC" from AddColumn above,
            // which is wrong for every one of them). Matches DbInitializer.CitySeed.
            migrationBuilder.Sql(@"
                UPDATE ""Cities"" SET ""TimeZoneId"" = CASE ""Name""
                    WHEN 'Baku' THEN 'Asia/Baku'
                    WHEN 'Istanbul' THEN 'Europe/Istanbul'
                    WHEN 'Dubai' THEN 'Asia/Dubai'
                    WHEN 'London' THEN 'Europe/London'
                    WHEN 'Paris' THEN 'Europe/Paris'
                    WHEN 'New York' THEN 'America/New_York'
                    WHEN 'Los Angeles' THEN 'America/Los_Angeles'
                    WHEN 'Tokyo' THEN 'Asia/Tokyo'
                    WHEN 'Beijing' THEN 'Asia/Shanghai'
                    WHEN 'Sydney' THEN 'Australia/Sydney'
                    WHEN 'Berlin' THEN 'Europe/Berlin'
                    WHEN 'Frankfurt' THEN 'Europe/Berlin'
                    WHEN 'Amsterdam' THEN 'Europe/Amsterdam'
                    WHEN 'Madrid' THEN 'Europe/Madrid'
                    WHEN 'Barcelona' THEN 'Europe/Madrid'
                    WHEN 'Rome' THEN 'Europe/Rome'
                    WHEN 'Milan' THEN 'Europe/Rome'
                    WHEN 'Vienna' THEN 'Europe/Vienna'
                    WHEN 'Zurich' THEN 'Europe/Zurich'
                    WHEN 'Moscow' THEN 'Europe/Moscow'
                    WHEN 'Saint Petersburg' THEN 'Europe/Moscow'
                    WHEN 'Tbilisi' THEN 'Asia/Tbilisi'
                    WHEN 'Yerevan' THEN 'Asia/Yerevan'
                    WHEN 'Ankara' THEN 'Europe/Istanbul'
                    WHEN 'Antalya' THEN 'Europe/Istanbul'
                    WHEN 'Abu Dhabi' THEN 'Asia/Dubai'
                    WHEN 'Doha' THEN 'Asia/Qatar'
                    WHEN 'Riyadh' THEN 'Asia/Riyadh'
                    WHEN 'Kuwait City' THEN 'Asia/Kuwait'
                    WHEN 'Muscat' THEN 'Asia/Muscat'
                    WHEN 'Cairo' THEN 'Africa/Cairo'
                    WHEN 'Casablanca' THEN 'Africa/Casablanca'
                    WHEN 'Lagos' THEN 'Africa/Lagos'
                    WHEN 'Nairobi' THEN 'Africa/Nairobi'
                    WHEN 'Mumbai' THEN 'Asia/Kolkata'
                    WHEN 'Delhi' THEN 'Asia/Kolkata'
                    WHEN 'Bangkok' THEN 'Asia/Bangkok'
                    WHEN 'Singapore' THEN 'Asia/Singapore'
                    WHEN 'Kuala Lumpur' THEN 'Asia/Kuala_Lumpur'
                    WHEN 'Seoul' THEN 'Asia/Seoul'
                    WHEN 'Hong Kong' THEN 'Asia/Hong_Kong'
                    WHEN 'Toronto' THEN 'America/Toronto'
                    WHEN 'Vancouver' THEN 'America/Vancouver'
                    WHEN 'Mexico City' THEN 'America/Mexico_City'
                    WHEN 'São Paulo' THEN 'America/Sao_Paulo'
                    WHEN 'Buenos Aires' THEN 'America/Argentina/Buenos_Aires'
                    WHEN 'Warsaw' THEN 'Europe/Warsaw'
                    WHEN 'Prague' THEN 'Europe/Prague'
                    WHEN 'Budapest' THEN 'Europe/Budapest'
                    WHEN 'Athens' THEN 'Europe/Athens'
                    ELSE ""TimeZoneId""
                END;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeZoneId",
                table: "Cities");
        }
    }
}
