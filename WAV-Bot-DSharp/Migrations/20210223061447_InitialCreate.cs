using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WAV_Bot_DSharp.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrackedUsers",
                columns: table => new
                {
                    Id = table.Column<ulong>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BanchoId = table.Column<int>(nullable: true),
                    BanchoTrackRecent = table.Column<bool>(nullable: false),
                    BanchoRecentLastAt = table.Column<DateTime>(nullable: true),
                    BanchoTrackTop = table.Column<bool>(nullable: false),
                    BanchoTopLastAt = table.Column<DateTime>(nullable: true),
                    GatariId = table.Column<int>(nullable: true),
                    GatariTrackRecent = table.Column<bool>(nullable: false),
                    GatariRecentLastAt = table.Column<DateTime>(nullable: true),
                    GatariTrackTop = table.Column<bool>(nullable: false),
                    GatariTopLastAt = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackedUsers", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrackedUsers");
        }
    }
}
