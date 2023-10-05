using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Migrations
{
    public partial class version3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "PriceDiscount",
                table: "OrderCoupon",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AlterColumn<int>(
                name: "Quantity",
                table: "Order",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.UpdateData(
                table: "BusinessFee",
                keyColumn: "BusinessFeeId",
                keyValue: 1L,
                column: "StartDate",
                value: new DateTime(2023, 10, 6, 0, 7, 9, 15, DateTimeKind.Local).AddTicks(9824));

            migrationBuilder.UpdateData(
                table: "Category",
                keyColumn: "CategoryId",
                keyValue: 2L,
                column: "CategoryName",
                value: "Giáo dục");

            migrationBuilder.UpdateData(
                table: "Category",
                keyColumn: "CategoryId",
                keyValue: 3L,
                column: "CategoryName",
                value: "Trò chơi");

            migrationBuilder.InsertData(
                table: "Category",
                columns: new[] { "CategoryId", "CategoryName" },
                values: new object[,]
                {
                    { 4L, "VPS" },
                    { 5L, "Khác" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Category",
                keyColumn: "CategoryId",
                keyValue: 4L);

            migrationBuilder.DeleteData(
                table: "Category",
                keyColumn: "CategoryId",
                keyValue: 5L);

            migrationBuilder.DropColumn(
                name: "PriceDiscount",
                table: "OrderCoupon");

            migrationBuilder.AlterColumn<long>(
                name: "Quantity",
                table: "Order",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.UpdateData(
                table: "BusinessFee",
                keyColumn: "BusinessFeeId",
                keyValue: 1L,
                column: "StartDate",
                value: new DateTime(2023, 10, 4, 21, 42, 45, 617, DateTimeKind.Local).AddTicks(565));

            migrationBuilder.UpdateData(
                table: "Category",
                keyColumn: "CategoryId",
                keyValue: 2L,
                column: "CategoryName",
                value: "VPS");

            migrationBuilder.UpdateData(
                table: "Category",
                keyColumn: "CategoryId",
                keyValue: 3L,
                column: "CategoryName",
                value: "Khác");
        }
    }
}
