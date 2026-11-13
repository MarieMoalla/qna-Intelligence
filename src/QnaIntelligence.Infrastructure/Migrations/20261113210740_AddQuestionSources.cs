using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QnaIntelligence.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionSources : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuestionSources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImportedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionSources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionSources_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionSources_QuestionId",
                table: "QuestionSources",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionSources_SourceId",
                table: "QuestionSources",
                column: "SourceId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuestionSources");
        }
    }
}
