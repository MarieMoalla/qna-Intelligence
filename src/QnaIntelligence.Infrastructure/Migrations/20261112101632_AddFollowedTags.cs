using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QnaIntelligence.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFollowedTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FollowedTags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FollowedTags", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FollowedTags_Name",
                table: "FollowedTags",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FollowedTags");
        }
    }
}
