using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlyzenApi.Persistence.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddPaidMealPricesAndNoMealOption : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Reprice the previously-free meals for already-seeded databases (fresh
            // installs get these prices straight from DbInitializer's seed list instead).
            migrationBuilder.Sql("UPDATE \"MealOptions\" SET \"Price\" = 15 WHERE \"Type\" = 0 AND \"Price\" = 0;");
            migrationBuilder.Sql("UPDATE \"MealOptions\" SET \"Price\" = 12 WHERE \"Type\" = 1 AND \"Price\" = 0;");
            migrationBuilder.Sql("UPDATE \"MealOptions\" SET \"Price\" = 14 WHERE \"Type\" = 2 AND \"Price\" = 0;");
            migrationBuilder.Sql("UPDATE \"MealOptions\" SET \"Price\" = 13 WHERE \"Type\" = 3 AND \"Price\" = 0;");
            migrationBuilder.Sql("UPDATE \"MealOptions\" SET \"Price\" = 16 WHERE \"Type\" = 4 AND \"Price\" = 0;");

            // Only insert "No Meal" (Type=5/None) into databases that were already
            // seeded before this feature existed - a genuinely fresh database has an
            // empty MealOptions table at migration time and gets it from DbInitializer's
            // seed list instead, so this guard avoids DbInitializer's AnyAsync() check
            // seeing a non-empty table and skipping the rest of the fresh seed data.
            migrationBuilder.Sql(@"
                INSERT INTO ""MealOptions"" (""Id"", ""Type"", ""Name"", ""Description"", ""Price"", ""Currency"", ""CreatedAt"")
                SELECT gen_random_uuid(), 5, 'No Meal', 'Skip in-flight meal service.', 0, 'AZN', now()
                WHERE EXISTS (SELECT 1 FROM ""MealOptions"" WHERE ""Type"" = 0)
                  AND NOT EXISTS (SELECT 1 FROM ""MealOptions"" WHERE ""Type"" = 5);
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
