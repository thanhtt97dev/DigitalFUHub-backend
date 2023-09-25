using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Migrations
{
    public partial class updaterelationship : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_WithdrawTransaction_UserId",
                table: "WithdrawTransaction",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_WithdrawTransaction_User_UserId",
                table: "WithdrawTransaction",
                column: "UserId",
                principalTable: "User",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WithdrawTransaction_User_UserId",
                table: "WithdrawTransaction");

            migrationBuilder.DropIndex(
                name: "IX_WithdrawTransaction_UserId",
                table: "WithdrawTransaction");
        }
    }
}
