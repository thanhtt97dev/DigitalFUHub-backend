using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Migrations
{
    public partial class version1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ShopRegisterFeeId",
                table: "Shop",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "ShopRegisterFee",
                columns: table => new
                {
                    ShopRegisterFeeId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fee = table.Column<long>(type: "bigint", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShopRegisterFee", x => x.ShopRegisterFeeId);
                });

            migrationBuilder.UpdateData(
                table: "BusinessFee",
                keyColumn: "BusinessFeeId",
                keyValue: 1L,
                column: "StartDate",
                value: new DateTime(2023, 12, 20, 9, 2, 54, 312, DateTimeKind.Local).AddTicks(5602));

            migrationBuilder.UpdateData(
                table: "FeedbackBenefit",
                keyColumn: "FeedbackBenefitId",
                keyValue: 1L,
                column: "StartDate",
                value: new DateTime(2023, 12, 20, 9, 2, 54, 312, DateTimeKind.Local).AddTicks(5616));

            migrationBuilder.InsertData(
                table: "ShopRegisterFee",
                columns: new[] { "ShopRegisterFeeId", "EndDate", "Fee", "StartDate" },
                values: new object[] { 1L, null, 50000L, new DateTime(2023, 12, 20, 9, 2, 54, 312, DateTimeKind.Local).AddTicks(5578) });

            migrationBuilder.CreateIndex(
                name: "IX_Shop_ShopRegisterFeeId",
                table: "Shop",
                column: "ShopRegisterFeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Shop_ShopRegisterFee_ShopRegisterFeeId",
                table: "Shop",
                column: "ShopRegisterFeeId",
                principalTable: "ShopRegisterFee",
                principalColumn: "ShopRegisterFeeId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Shop_ShopRegisterFee_ShopRegisterFeeId",
                table: "Shop");

            migrationBuilder.DropTable(
                name: "ShopRegisterFee");

            migrationBuilder.DropIndex(
                name: "IX_Shop_ShopRegisterFeeId",
                table: "Shop");

            migrationBuilder.DropColumn(
                name: "ShopRegisterFeeId",
                table: "Shop");

            migrationBuilder.UpdateData(
                table: "BusinessFee",
                keyColumn: "BusinessFeeId",
                keyValue: 1L,
                column: "StartDate",
                value: new DateTime(2023, 12, 16, 16, 7, 38, 480, DateTimeKind.Local).AddTicks(4490));

            migrationBuilder.UpdateData(
                table: "FeedbackBenefit",
                keyColumn: "FeedbackBenefitId",
                keyValue: 1L,
                column: "StartDate",
                value: new DateTime(2023, 12, 16, 16, 7, 38, 480, DateTimeKind.Local).AddTicks(4516));
        }
    }
}
