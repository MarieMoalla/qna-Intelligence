using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QnaIntelligence.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSeedQuestionPriorityScores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE [Questions] SET [PriorityScore] = 83 WHERE [Title] = N'ASP.NET Core startup exception'; " +
                "UPDATE [Questions] SET [PriorityScore] = 72 WHERE [Title] = N'Windows Update error 0x80070057';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE [Questions] SET [PriorityScore] = 82 WHERE [Title] = N'ASP.NET Core startup exception'; " +
                "UPDATE [Questions] SET [PriorityScore] = 71 WHERE [Title] = N'Windows Update error 0x80070057';");
        }
    }
}
