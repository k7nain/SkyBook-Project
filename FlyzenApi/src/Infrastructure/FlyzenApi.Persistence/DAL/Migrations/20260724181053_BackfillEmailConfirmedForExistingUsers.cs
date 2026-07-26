using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlyzenApi.Persistence.DAL.Migrations
{
    /// <inheritdoc />
    public partial class BackfillEmailConfirmedForExistingUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Accounts created before email verification existed were never asked to
            // verify, so treat them as already confirmed instead of locking them out.
            migrationBuilder.Sql("UPDATE \"Users\" SET \"IsEmailConfirmed\" = TRUE;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
