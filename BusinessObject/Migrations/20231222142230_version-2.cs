using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Migrations
{
    public partial class version2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPay",
                table: "DepositTransaction");

            migrationBuilder.AddColumn<int>(
                name: "DepositTransactionStatusId",
                table: "DepositTransaction",
                type: "int",
                nullable: false,
                defaultValue: 0);

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepositTransactionStatusId",
                table: "DepositTransaction");

            migrationBuilder.AddColumn<bool>(
                name: "IsPay",
                table: "DepositTransaction",
                type: "bit",
                nullable: false,
                defaultValue: false);

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

            migrationBuilder.UpdateData(
                table: "ShopRegisterFee",
                keyColumn: "ShopRegisterFeeId",
                keyValue: 1L,
                column: "StartDate",
                value: new DateTime(2023, 12, 20, 9, 2, 54, 312, DateTimeKind.Local).AddTicks(5578));
        }
    }
}
