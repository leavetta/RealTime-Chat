using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chat.Web.Migrations
{
    public partial class NewChatMember : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChatMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoomIdId = table.Column<int>(type: "int", nullable: true),
                    UserIdId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMembers_AspNetUsers_UserIdId",
                        column: x => x.UserIdId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ChatMembers_Rooms_RoomIdId",
                        column: x => x.RoomIdId,
                        principalTable: "Rooms",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMembers_RoomIdId",
                table: "ChatMembers",
                column: "RoomIdId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMembers_UserIdId",
                table: "ChatMembers",
                column: "UserIdId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatMembers");
        }
    }
}
