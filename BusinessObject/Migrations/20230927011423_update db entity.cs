using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Migrations
{
    public partial class updatedbentity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Media_Feedback_ForeignId",
                table: "Media");

            migrationBuilder.DropForeignKey(
                name: "FK_Media_Product_ForeignId",
                table: "Media");

            migrationBuilder.DropIndex(
                name: "IX_Media_ForeignId",
                table: "Media");

            migrationBuilder.AddColumn<long>(
                name: "FeedbackId",
                table: "Media",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ProductId",
                table: "Media",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Media_FeedbackId",
                table: "Media",
                column: "FeedbackId");

            migrationBuilder.CreateIndex(
                name: "IX_Media_ProductId",
                table: "Media",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Media_Feedback_FeedbackId",
                table: "Media",
                column: "FeedbackId",
                principalTable: "Feedback",
                principalColumn: "FeedbackId");

            migrationBuilder.AddForeignKey(
                name: "FK_Media_Product_ProductId",
                table: "Media",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "ProductId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Media_Feedback_FeedbackId",
                table: "Media");

            migrationBuilder.DropForeignKey(
                name: "FK_Media_Product_ProductId",
                table: "Media");

            migrationBuilder.DropIndex(
                name: "IX_Media_FeedbackId",
                table: "Media");

            migrationBuilder.DropIndex(
                name: "IX_Media_ProductId",
                table: "Media");

            migrationBuilder.DropColumn(
                name: "FeedbackId",
                table: "Media");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "Media");

            migrationBuilder.CreateIndex(
                name: "IX_Media_ForeignId",
                table: "Media",
                column: "ForeignId");

            migrationBuilder.AddForeignKey(
                name: "FK_Media_Feedback_ForeignId",
                table: "Media",
                column: "ForeignId",
                principalTable: "Feedback",
                principalColumn: "FeedbackId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Media_Product_ForeignId",
                table: "Media",
                column: "ForeignId",
                principalTable: "Product",
                principalColumn: "ProductId");
        }
    }
}
