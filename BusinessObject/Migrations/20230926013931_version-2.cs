using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Migrations
{
    public partial class version2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssetInformation_ProductType_ProductVariantId",
                table: "AssetInformation");

            migrationBuilder.DropForeignKey(
                name: "FK_Cart_ProductType_ProductTypeId",
                table: "Cart");

            migrationBuilder.DropForeignKey(
                name: "FK_Order_ProductType_ProductVariantId",
                table: "Order");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductType_Product_ProductId",
                table: "ProductType");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductType",
                table: "ProductType");

            migrationBuilder.RenameTable(
                name: "ProductType",
                newName: "ProductVariant");

            migrationBuilder.RenameIndex(
                name: "IX_ProductType_ProductId",
                table: "ProductVariant",
                newName: "IX_ProductVariant_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductVariant",
                table: "ProductVariant",
                column: "ProductVariantId");

            migrationBuilder.CreateTable(
                name: "DepositeTransactionBill",
                columns: table => new
                {
                    DepositeTransactionBillId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepositTransactionId = table.Column<long>(type: "bigint", nullable: false),
                    PostingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ÁccountNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreditAmount = table.Column<int>(type: "int", nullable: false),
                    DebitAmount = table.Column<int>(type: "int", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AvailableBalance = table.Column<int>(type: "int", nullable: false),
                    BeneficiaryAccount = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BenAccountName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BankName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BenAccountNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DocId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransactionType = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepositeTransactionBill", x => x.DepositeTransactionBillId);
                    table.ForeignKey(
                        name: "FK_DepositeTransactionBill_DepositTransaction_DepositTransactionId",
                        column: x => x.DepositTransactionId,
                        principalTable: "DepositTransaction",
                        principalColumn: "DepositTransactionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WithdrawTransactionBill",
                columns: table => new
                {
                    WidrawTransactionBillId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WithdrawTransactionId = table.Column<long>(type: "bigint", nullable: false),
                    PostingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TransactionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AccountNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreditAmount = table.Column<int>(type: "int", nullable: false),
                    DebitAmount = table.Column<int>(type: "int", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AvailableBalance = table.Column<int>(type: "int", nullable: false),
                    BeneficiaryAccount = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BenAccountName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BankName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BenAccountNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DocId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TransactionType = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WithdrawTransactionBill", x => x.WidrawTransactionBillId);
                    table.ForeignKey(
                        name: "FK_WithdrawTransactionBill_WithdrawTransaction_WithdrawTransactionId",
                        column: x => x.WithdrawTransactionId,
                        principalTable: "WithdrawTransaction",
                        principalColumn: "WithdrawTransactionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DepositeTransactionBill_DepositTransactionId",
                table: "DepositeTransactionBill",
                column: "DepositTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_WithdrawTransactionBill_WithdrawTransactionId",
                table: "WithdrawTransactionBill",
                column: "WithdrawTransactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssetInformation_ProductVariant_ProductVariantId",
                table: "AssetInformation",
                column: "ProductVariantId",
                principalTable: "ProductVariant",
                principalColumn: "ProductVariantId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cart_ProductVariant_ProductTypeId",
                table: "Cart",
                column: "ProductTypeId",
                principalTable: "ProductVariant",
                principalColumn: "ProductVariantId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Order_ProductVariant_ProductVariantId",
                table: "Order",
                column: "ProductVariantId",
                principalTable: "ProductVariant",
                principalColumn: "ProductVariantId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductVariant_Product_ProductId",
                table: "ProductVariant",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssetInformation_ProductVariant_ProductVariantId",
                table: "AssetInformation");

            migrationBuilder.DropForeignKey(
                name: "FK_Cart_ProductVariant_ProductTypeId",
                table: "Cart");

            migrationBuilder.DropForeignKey(
                name: "FK_Order_ProductVariant_ProductVariantId",
                table: "Order");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductVariant_Product_ProductId",
                table: "ProductVariant");

            migrationBuilder.DropTable(
                name: "DepositeTransactionBill");

            migrationBuilder.DropTable(
                name: "WithdrawTransactionBill");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductVariant",
                table: "ProductVariant");

            migrationBuilder.RenameTable(
                name: "ProductVariant",
                newName: "ProductType");

            migrationBuilder.RenameIndex(
                name: "IX_ProductVariant_ProductId",
                table: "ProductType",
                newName: "IX_ProductType_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductType",
                table: "ProductType",
                column: "ProductVariantId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssetInformation_ProductType_ProductVariantId",
                table: "AssetInformation",
                column: "ProductVariantId",
                principalTable: "ProductType",
                principalColumn: "ProductVariantId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cart_ProductType_ProductTypeId",
                table: "Cart",
                column: "ProductTypeId",
                principalTable: "ProductType",
                principalColumn: "ProductVariantId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Order_ProductType_ProductVariantId",
                table: "Order",
                column: "ProductVariantId",
                principalTable: "ProductType",
                principalColumn: "ProductVariantId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductType_Product_ProductId",
                table: "ProductType",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
