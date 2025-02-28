using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SkillBridgeAPI.Migrations
{
    /// <inheritdoc />
    public partial class UserAvatarNumberAndRatingAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte>(
                name: "login_attempts",
                table: "user",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0,
                oldClrType: typeof(short),
                oldType: "smallint",
                oldDefaultValue: (short)0);

            migrationBuilder.AddColumn<byte>(
                name: "avatar_number",
                table: "user",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "rating",
                table: "user",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "avatar_number",
                table: "user");

            migrationBuilder.DropColumn(
                name: "rating",
                table: "user");

            migrationBuilder.AlterColumn<short>(
                name: "login_attempts",
                table: "user",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0,
                oldClrType: typeof(byte),
                oldType: "smallint",
                oldDefaultValue: (byte)0);
        }
    }
}
