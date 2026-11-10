using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QnaIntelligence.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionPriorityScore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PriorityScore",
                table: "Questions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                "UPDATE [Questions] SET [PriorityScore] = 95 WHERE [Title] = N'Azure App Service deployment fails'; " +
                "UPDATE [Questions] SET [PriorityScore] = 82 WHERE [Title] = N'ASP.NET Core startup exception'; " +
                "UPDATE [Questions] SET [PriorityScore] = 71 WHERE [Title] = N'Windows Update error 0x80070057';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PriorityScore",
                table: "Questions");
        }
    }
}
