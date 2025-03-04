using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SkillBridgeAPI.Migrations
{
    /// <inheritdoc />
    public partial class ModelsWasRefactored : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "refresh_token",
                columns: table => new
                {
                    TokenId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    expired_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("token_id", x => x.TokenId);
                });

            migrationBuilder.CreateTable(
                name: "skill",
                columns: table => new
                {
                    skill_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    skill_name = table.Column<string>(type: "text", nullable: false),
                    category = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("skill_pkey", x => x.skill_id);
                });

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    user_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    email = table.Column<string>(type: "text", nullable: false),
                    pwd_hash = table.Column<string>(type: "text", nullable: false),
                    first_name = table.Column<string>(type: "text", nullable: true),
                    last_name = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    username = table.Column<string>(type: "text", nullable: false),
                    subscription_status = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    ulid = table.Column<string>(type: "text", nullable: false),
                    login_attempts = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    next_attempt_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    avatar_number = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    rating = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("User_pkey", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "exchange",
                columns: table => new
                {
                    exchange_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id_1 = table.Column<long>(type: "bigint", nullable: false),
                    user_id_2 = table.Column<long>(type: "bigint", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SkillId = table.Column<long>(type: "bigint", nullable: true),
                    SkillId1 = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("exchange_pkey", x => x.exchange_id);
                    table.ForeignKey(
                        name: "FK_exchange_skill_SkillId",
                        column: x => x.SkillId,
                        principalTable: "skill",
                        principalColumn: "skill_id");
                    table.ForeignKey(
                        name: "FK_exchange_skill_SkillId1",
                        column: x => x.SkillId1,
                        principalTable: "skill",
                        principalColumn: "skill_id");
                    table.ForeignKey(
                        name: "exchange_user_id_1_fkey",
                        column: x => x.user_id_1,
                        principalTable: "user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "exchange_user_id_2_fkey",
                        column: x => x.user_id_2,
                        principalTable: "user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "userskill",
                columns: table => new
                {
                    user_skill_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    skill_id = table.Column<long>(type: "bigint", nullable: false),
                    skill_type = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("userskill_pkey", x => x.user_skill_id);
                    table.ForeignKey(
                        name: "userskill_skill_id_fkey",
                        column: x => x.skill_id,
                        principalTable: "skill",
                        principalColumn: "skill_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "userskill_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "chat",
                columns: table => new
                {
                    chat_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    chat_name = table.Column<string>(type: "text", nullable: true),
                    exchange_id = table.Column<long>(type: "bigint", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("chat_pkey", x => x.chat_id);
                    table.ForeignKey(
                        name: "chat_exchange_id_fkey",
                        column: x => x.exchange_id,
                        principalTable: "exchange",
                        principalColumn: "exchange_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "message",
                columns: table => new
                {
                    message_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    chat_id = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    message = table.Column<string>(type: "text", nullable: true),
                    sent_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    message_type = table.Column<string>(type: "text", nullable: true),
                    file_url = table.Column<string>(type: "text", nullable: true),
                    is_read = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("message_pkey", x => x.message_id);
                    table.ForeignKey(
                        name: "message_chat_id_fkey",
                        column: x => x.chat_id,
                        principalTable: "chat",
                        principalColumn: "chat_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "message_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sessionchat",
                columns: table => new
                {
                    exchange_id = table.Column<long>(type: "bigint", nullable: false),
                    chat_id = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("sessionchat_pkey", x => new { x.exchange_id, x.chat_id });
                    table.ForeignKey(
                        name: "sessionchat_chat_id_fkey",
                        column: x => x.chat_id,
                        principalTable: "chat",
                        principalColumn: "chat_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "sessionchat_exchange_id_fkey",
                        column: x => x.exchange_id,
                        principalTable: "exchange",
                        principalColumn: "exchange_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reaction",
                columns: table => new
                {
                    reaction_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    message_id = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    reaction_type = table.Column<string>(type: "text", nullable: true),
                    created_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("reaction_pkey", x => x.reaction_id);
                    table.ForeignKey(
                        name: "reaction_message_id_fkey",
                        column: x => x.message_id,
                        principalTable: "message",
                        principalColumn: "message_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "reaction_user_id_fkey",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_chat_exchange_id",
                table: "chat",
                column: "exchange_id");

            migrationBuilder.CreateIndex(
                name: "IX_exchange_SkillId",
                table: "exchange",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_exchange_SkillId1",
                table: "exchange",
                column: "SkillId1");

            migrationBuilder.CreateIndex(
                name: "IX_exchange_user_id_1",
                table: "exchange",
                column: "user_id_1");

            migrationBuilder.CreateIndex(
                name: "IX_exchange_user_id_2",
                table: "exchange",
                column: "user_id_2");

            migrationBuilder.CreateIndex(
                name: "IX_message_chat_id",
                table: "message",
                column: "chat_id");

            migrationBuilder.CreateIndex(
                name: "IX_message_user_id",
                table: "message",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_reaction_message_id",
                table: "reaction",
                column: "message_id");

            migrationBuilder.CreateIndex(
                name: "IX_reaction_user_id",
                table: "reaction",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_sessionchat_chat_id",
                table: "sessionchat",
                column: "chat_id");

            migrationBuilder.CreateIndex(
                name: "User_email_key",
                table: "user",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_userskill_skill_id",
                table: "userskill",
                column: "skill_id");

            migrationBuilder.CreateIndex(
                name: "IX_userskill_user_id",
                table: "userskill",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "reaction");

            migrationBuilder.DropTable(
                name: "refresh_token");

            migrationBuilder.DropTable(
                name: "sessionchat");

            migrationBuilder.DropTable(
                name: "userskill");

            migrationBuilder.DropTable(
                name: "message");

            migrationBuilder.DropTable(
                name: "chat");

            migrationBuilder.DropTable(
                name: "exchange");

            migrationBuilder.DropTable(
                name: "skill");

            migrationBuilder.DropTable(
                name: "user");
        }
    }
}
