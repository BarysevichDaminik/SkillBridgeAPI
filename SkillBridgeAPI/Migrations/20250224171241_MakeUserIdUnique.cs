using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillBridgeAPI.Migrations
{
    /// <inheritdoc />
    public partial class MakeUserIdUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_refresh_token_user_id",
                table: "refresh_token",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_refresh_token_user_id",
                table: "refresh_token");
        }
    }
}
