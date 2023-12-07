using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Migrations
{
    public partial class version1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                table: "User",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "BusinessFee",
                keyColumn: "BusinessFeeId",
                keyValue: 1L,
                column: "StartDate",
                value: new DateTime(2023, 12, 7, 20, 37, 36, 741, DateTimeKind.Local).AddTicks(2412));

            migrationBuilder.UpdateData(
                table: "FeedbackBenefit",
                keyColumn: "FeedbackBenefitId",
                keyValue: 1,
                column: "StartDate",
                value: new DateTime(2023, 12, 7, 20, 37, 36, 741, DateTimeKind.Local).AddTicks(2438));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreateDate",
                table: "User");

            migrationBuilder.UpdateData(
                table: "BusinessFee",
                keyColumn: "BusinessFeeId",
                keyValue: 1L,
                column: "StartDate",
                value: new DateTime(2023, 12, 5, 21, 36, 0, 836, DateTimeKind.Local).AddTicks(5649));

            migrationBuilder.UpdateData(
                table: "FeedbackBenefit",
                keyColumn: "FeedbackBenefitId",
                keyValue: 1,
                column: "StartDate",
                value: new DateTime(2023, 12, 5, 21, 36, 0, 836, DateTimeKind.Local).AddTicks(5675));
        }
    }
}
