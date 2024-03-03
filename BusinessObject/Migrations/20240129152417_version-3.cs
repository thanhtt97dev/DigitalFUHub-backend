using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Migrations
{
    public partial class version3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "BusinessFee",
                keyColumn: "BusinessFeeId",
                keyValue: 1L,
                column: "StartDate",
                value: new DateTime(2024, 1, 29, 22, 24, 16, 159, DateTimeKind.Local).AddTicks(769));

            migrationBuilder.UpdateData(
                table: "FeedbackBenefit",
                keyColumn: "FeedbackBenefitId",
                keyValue: 1L,
                column: "StartDate",
                value: new DateTime(2024, 1, 29, 22, 24, 16, 159, DateTimeKind.Local).AddTicks(782));

            migrationBuilder.InsertData(
                table: "OrderStatus",
                columns: new[] { "OrderStatusId", "Name" },
                values: new object[,]
                {
                    { 8L, "In prossess" },
                    { 9L, "Failed" }
                });

            migrationBuilder.UpdateData(
                table: "ShopRegisterFee",
                keyColumn: "ShopRegisterFeeId",
                keyValue: 1L,
                column: "StartDate",
                value: new DateTime(2024, 1, 29, 22, 24, 16, 159, DateTimeKind.Local).AddTicks(747));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "OrderStatus",
                keyColumn: "OrderStatusId",
                keyValue: 8L);

            migrationBuilder.DeleteData(
                table: "OrderStatus",
                keyColumn: "OrderStatusId",
                keyValue: 9L);

            migrationBuilder.UpdateData(
                table: "BusinessFee",
                keyColumn: "BusinessFeeId",
                keyValue: 1L,
                column: "StartDate",
                value: new DateTime(2023, 12, 22, 21, 22, 29, 447, DateTimeKind.Local).AddTicks(7018));

            migrationBuilder.UpdateData(
                table: "FeedbackBenefit",
                keyColumn: "FeedbackBenefitId",
                keyValue: 1L,
                column: "StartDate",
                value: new DateTime(2023, 12, 22, 21, 22, 29, 447, DateTimeKind.Local).AddTicks(7043));

            migrationBuilder.UpdateData(
                table: "ShopRegisterFee",
                keyColumn: "ShopRegisterFeeId",
                keyValue: 1L,
                column: "StartDate",
                value: new DateTime(2023, 12, 22, 21, 22, 29, 447, DateTimeKind.Local).AddTicks(6980));
        }
    }
}
