using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bank",
                columns: table => new
                {
                    BankId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BankCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    isActivate = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bank", x => x.BankId);
                });

            migrationBuilder.CreateTable(
                name: "BusinessFee",
                columns: table => new
                {
                    BusinessFeeId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fee = table.Column<long>(type: "bigint", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessFee", x => x.BusinessFeeId);
                });

            migrationBuilder.CreateTable(
                name: "Category",
                columns: table => new
                {
                    CategoryId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Category", x => x.CategoryId);
                });

            migrationBuilder.CreateTable(
                name: "Conversations",
                columns: table => new
                {
                    ConversationId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConversationName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateCreate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActivate = table.Column<bool>(type: "bit", nullable: false),
                    IsGroup = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversations", x => x.ConversationId);
                });

            migrationBuilder.CreateTable(
                name: "CouponType",
                columns: table => new
                {
                    CouponTypeId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouponType", x => x.CouponTypeId);
                });

            migrationBuilder.CreateTable(
                name: "FeedbackBenefit",
                columns: table => new
                {
                    FeedbackBenefitId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Coin = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackBenefit", x => x.FeedbackBenefitId);
                });

            migrationBuilder.CreateTable(
                name: "OrderStatus",
                columns: table => new
                {
                    OrderStatusId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderStatus", x => x.OrderStatusId);
                });

            migrationBuilder.CreateTable(
                name: "ProductStatus",
                columns: table => new
                {
                    ProductStatusId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductStatusName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductStatus", x => x.ProductStatusId);
                });

            migrationBuilder.CreateTable(
                name: "ReasonReportProduct",
                columns: table => new
                {
                    ReasonReportProductId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ViName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ViExplanation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnExplanation = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReasonReportProduct", x => x.ReasonReportProductId);
                });

            migrationBuilder.CreateTable(
                name: "ReportProductStatus",
                columns: table => new
                {
                    ReportProductId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportProductStatus", x => x.ReportProductId);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    RoleId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "TransactionCoinType",
                columns: table => new
                {
                    TransactionCoinTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionCoinType", x => x.TransactionCoinTypeId);
                });

            migrationBuilder.CreateTable(
                name: "TransactionInternalType",
                columns: table => new
                {
                    TransactionInternalTypeId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionInternalType", x => x.TransactionInternalTypeId);
                });

            migrationBuilder.CreateTable(
                name: "WithdrawTransactionStatus",
                columns: table => new
                {
                    WithdrawTransactionStatusId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WithdrawTransactionStatus", x => x.WithdrawTransactionStatusId);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<long>(type: "bigint", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Fullname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Avatar = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorAuthentication = table.Column<bool>(type: "bit", nullable: false),
                    AccountBalance = table.Column<long>(type: "bigint", nullable: false),
                    Coin = table.Column<long>(type: "bigint", nullable: false),
                    SignInGoogle = table.Column<bool>(type: "bit", nullable: false),
                    IsConfirm = table.Column<bool>(type: "bit", nullable: false),
                    IsChangeUsername = table.Column<bool>(type: "bit", nullable: false),
                    LastTimeOnline = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsOnline = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_User_Role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccessToken",
                columns: table => new
                {
                    AccessTokenId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    JwtId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpiredDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRevoked = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessToken", x => x.AccessTokenId);
                    table.ForeignKey(
                        name: "FK_AccessToken_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DepositTransaction",
                columns: table => new
                {
                    DepositTransactionId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaidDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<long>(type: "bigint", nullable: false),
                    IsPay = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepositTransaction", x => x.DepositTransactionId);
                    table.ForeignKey(
                        name: "FK_DepositTransaction_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    MessageId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ConversationId = table.Column<long>(type: "bigint", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MessageType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateCreate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.MessageId);
                    table.ForeignKey(
                        name: "FK_Messages_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "ConversationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Messages_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    NotificationId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Link = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsReaded = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notification", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK_Notification_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshToken",
                columns: table => new
                {
                    RefreshTokenId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    TokenRefresh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpiredDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    JwtId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshToken", x => x.RefreshTokenId);
                    table.ForeignKey(
                        name: "FK_RefreshToken_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Shop",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ShopName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Avatar = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateCreate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shop", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Shop_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TwoFactorAuthentication",
                columns: table => new
                {
                    TwoFactorAuthenticationId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    SecretKey = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TwoFactorAuthentication", x => x.TwoFactorAuthenticationId);
                    table.ForeignKey(
                        name: "FK_TwoFactorAuthentication_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserBank",
                columns: table => new
                {
                    UserBankId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    BankId = table.Column<long>(type: "bigint", nullable: false),
                    CreditAccount = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreditAccountName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    isActivate = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBank", x => x.UserBankId);
                    table.ForeignKey(
                        name: "FK_UserBank_Bank_BankId",
                        column: x => x.BankId,
                        principalTable: "Bank",
                        principalColumn: "BankId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserBank_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserConversation",
                columns: table => new
                {
                    UserConversationId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ConversationId = table.Column<long>(type: "bigint", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserConversation", x => x.UserConversationId);
                    table.ForeignKey(
                        name: "FK_UserConversation_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "ConversationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserConversation_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cart",
                columns: table => new
                {
                    CartId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ShopId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cart", x => x.CartId);
                    table.ForeignKey(
                        name: "FK_Cart_Shop_ShopId",
                        column: x => x.ShopId,
                        principalTable: "Shop",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Cart_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Coupon",
                columns: table => new
                {
                    CouponId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShopId = table.Column<long>(type: "bigint", nullable: false),
                    CouponTypeId = table.Column<long>(type: "bigint", nullable: false),
                    CouponCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CouponName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PriceDiscount = table.Column<long>(type: "bigint", nullable: false),
                    Quantity = table.Column<long>(type: "bigint", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MinTotalOrderValue = table.Column<long>(type: "bigint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coupon", x => x.CouponId);
                    table.ForeignKey(
                        name: "FK_Coupon_CouponType_CouponTypeId",
                        column: x => x.CouponTypeId,
                        principalTable: "CouponType",
                        principalColumn: "CouponTypeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Coupon_Shop_ShopId",
                        column: x => x.ShopId,
                        principalTable: "Shop",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Order",
                columns: table => new
                {
                    OrderId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ShopId = table.Column<long>(type: "bigint", nullable: false),
                    BusinessFeeId = table.Column<long>(type: "bigint", nullable: false),
                    ConversationId = table.Column<long>(type: "bigint", nullable: true),
                    OrderStatusId = table.Column<long>(type: "bigint", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalAmount = table.Column<long>(type: "bigint", nullable: false),
                    TotalCouponDiscount = table.Column<long>(type: "bigint", nullable: false),
                    TotalCoinDiscount = table.Column<long>(type: "bigint", nullable: false),
                    TotalPayment = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_Order_BusinessFee_BusinessFeeId",
                        column: x => x.BusinessFeeId,
                        principalTable: "BusinessFee",
                        principalColumn: "BusinessFeeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Order_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "ConversationId");
                    table.ForeignKey(
                        name: "FK_Order_OrderStatus_OrderStatusId",
                        column: x => x.OrderStatusId,
                        principalTable: "OrderStatus",
                        principalColumn: "OrderStatusId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Order_Shop_ShopId",
                        column: x => x.ShopId,
                        principalTable: "Shop",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Order_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Product",
                columns: table => new
                {
                    ProductId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ShopId = table.Column<long>(type: "bigint", nullable: false),
                    CategoryId = table.Column<long>(type: "bigint", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Discount = table.Column<int>(type: "int", nullable: false),
                    Thumbnail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateCreate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateUpdate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalRatingStar = table.Column<long>(type: "bigint", nullable: false),
                    NumberFeedback = table.Column<long>(type: "bigint", nullable: false),
                    ViewCount = table.Column<int>(type: "int", nullable: false),
                    LikeCount = table.Column<int>(type: "int", nullable: false),
                    SoldCount = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProductStatusId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Product", x => x.ProductId);
                    table.ForeignKey(
                        name: "FK_Product_Category_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Category",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Product_ProductStatus_ProductStatusId",
                        column: x => x.ProductStatusId,
                        principalTable: "ProductStatus",
                        principalColumn: "ProductStatusId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Product_Shop_ShopId",
                        column: x => x.ShopId,
                        principalTable: "Shop",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WithdrawTransaction",
                columns: table => new
                {
                    WithdrawTransactionId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserBankId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    WithdrawTransactionStatusId = table.Column<long>(type: "bigint", nullable: false),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaidDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<long>(type: "bigint", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WithdrawTransaction", x => x.WithdrawTransactionId);
                    table.ForeignKey(
                        name: "FK_WithdrawTransaction_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WithdrawTransaction_UserBank_UserBankId",
                        column: x => x.UserBankId,
                        principalTable: "UserBank",
                        principalColumn: "UserBankId");
                    table.ForeignKey(
                        name: "FK_WithdrawTransaction_WithdrawTransactionStatus_WithdrawTransactionStatusId",
                        column: x => x.WithdrawTransactionStatusId,
                        principalTable: "WithdrawTransactionStatus",
                        principalColumn: "WithdrawTransactionStatusId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HistoryOrderStatus",
                columns: table => new
                {
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    OrderStatusId = table.Column<long>(type: "bigint", nullable: false),
                    DateCreate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryOrderStatus", x => new { x.OrderId, x.OrderStatusId });
                    table.ForeignKey(
                        name: "FK_HistoryOrderStatus_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Order",
                        principalColumn: "OrderId");
                    table.ForeignKey(
                        name: "FK_HistoryOrderStatus_OrderStatus_OrderStatusId",
                        column: x => x.OrderStatusId,
                        principalTable: "OrderStatus",
                        principalColumn: "OrderStatusId");
                });

            migrationBuilder.CreateTable(
                name: "OrderCoupon",
                columns: table => new
                {
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    CouponId = table.Column<long>(type: "bigint", nullable: false),
                    PriceDiscount = table.Column<long>(type: "bigint", nullable: false),
                    UseDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderCoupon", x => new { x.OrderId, x.CouponId });
                    table.ForeignKey(
                        name: "FK_OrderCoupon_Coupon_CouponId",
                        column: x => x.CouponId,
                        principalTable: "Coupon",
                        principalColumn: "CouponId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderCoupon_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Order",
                        principalColumn: "OrderId");
                });

            migrationBuilder.CreateTable(
                name: "TransactionInternal",
                columns: table => new
                {
                    TransactionInternalId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    TransactionInternalTypeId = table.Column<long>(type: "bigint", nullable: false),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    PaymentAmount = table.Column<long>(type: "bigint", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateCreate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionInternal", x => x.TransactionInternalId);
                    table.ForeignKey(
                        name: "FK_TransactionInternal_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Order",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransactionInternal_TransactionInternalType_TransactionInternalTypeId",
                        column: x => x.TransactionInternalTypeId,
                        principalTable: "TransactionInternalType",
                        principalColumn: "TransactionInternalTypeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransactionInternal_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "CouponProduct",
                columns: table => new
                {
                    CouponId = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActivate = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CouponProduct", x => new { x.CouponId, x.ProductId });
                    table.ForeignKey(
                        name: "FK_CouponProduct_Coupon_CouponId",
                        column: x => x.CouponId,
                        principalTable: "Coupon",
                        principalColumn: "CouponId");
                    table.ForeignKey(
                        name: "FK_CouponProduct_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "ProductId");
                });

            migrationBuilder.CreateTable(
                name: "ProductMedia",
                columns: table => new
                {
                    ProductMediaId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductMedia", x => x.ProductMediaId);
                    table.ForeignKey(
                        name: "FK_ProductMedia_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductVariant",
                columns: table => new
                {
                    ProductVariantId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Price = table.Column<long>(type: "bigint", nullable: false),
                    Discount = table.Column<int>(type: "int", nullable: false),
                    isActivate = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductVariant", x => x.ProductVariantId);
                    table.ForeignKey(
                        name: "FK_ProductVariant_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportProduct",
                columns: table => new
                {
                    ReportProductId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    ReasonReportProductId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateCreate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReportProductStatusId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportProduct", x => x.ReportProductId);
                    table.ForeignKey(
                        name: "FK_ReportProduct_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReportProduct_ReasonReportProduct_ReasonReportProductId",
                        column: x => x.ReasonReportProductId,
                        principalTable: "ReasonReportProduct",
                        principalColumn: "ReasonReportProductId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReportProduct_ReportProductStatus_ReportProductStatusId",
                        column: x => x.ReportProductStatusId,
                        principalTable: "ReportProductStatus",
                        principalColumn: "ReportProductId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReportProduct_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Tag",
                columns: table => new
                {
                    TagId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    TagName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tag", x => x.TagId);
                    table.ForeignKey(
                        name: "FK_Tag_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WishList",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    ProductId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WishList", x => new { x.UserId, x.ProductId });
                    table.ForeignKey(
                        name: "FK_WishList_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "ProductId");
                    table.ForeignKey(
                        name: "FK_WishList_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
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
                    BenAccountNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "CartDetail",
                columns: table => new
                {
                    CartDetailId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CartId = table.Column<long>(type: "bigint", nullable: false),
                    ProductVariantId = table.Column<long>(type: "bigint", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartDetail", x => x.CartDetailId);
                    table.ForeignKey(
                        name: "FK_CartDetail_Cart_CartId",
                        column: x => x.CartId,
                        principalTable: "Cart",
                        principalColumn: "CartId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CartDetail_ProductVariant_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariant",
                        principalColumn: "ProductVariantId");
                });

            migrationBuilder.CreateTable(
                name: "OrderDetail",
                columns: table => new
                {
                    OrderDetailId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<long>(type: "bigint", nullable: false),
                    ProductVariantId = table.Column<long>(type: "bigint", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<long>(type: "bigint", nullable: false),
                    Discount = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<long>(type: "bigint", nullable: false),
                    IsFeedback = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderDetail", x => x.OrderDetailId);
                    table.ForeignKey(
                        name: "FK_OrderDetail_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Order",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderDetail_ProductVariant_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariant",
                        principalColumn: "ProductVariantId");
                });

            migrationBuilder.CreateTable(
                name: "AssetInformation",
                columns: table => new
                {
                    AssetInformationId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductVariantId = table.Column<long>(type: "bigint", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Asset = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrderDetailId = table.Column<long>(type: "bigint", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetInformation", x => x.AssetInformationId);
                    table.ForeignKey(
                        name: "FK_AssetInformation_OrderDetail_OrderDetailId",
                        column: x => x.OrderDetailId,
                        principalTable: "OrderDetail",
                        principalColumn: "OrderDetailId");
                    table.ForeignKey(
                        name: "FK_AssetInformation_ProductVariant_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariant",
                        principalColumn: "ProductVariantId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Feedback",
                columns: table => new
                {
                    FeedbackId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    OrderDetailId = table.Column<long>(type: "bigint", nullable: false),
                    FeedbackBenefitId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Rate = table.Column<int>(type: "int", nullable: false),
                    DateCreate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateUpdate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedback", x => x.FeedbackId);
                    table.ForeignKey(
                        name: "FK_Feedback_FeedbackBenefit_FeedbackBenefitId",
                        column: x => x.FeedbackBenefitId,
                        principalTable: "FeedbackBenefit",
                        principalColumn: "FeedbackBenefitId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Feedback_OrderDetail_OrderDetailId",
                        column: x => x.OrderDetailId,
                        principalTable: "OrderDetail",
                        principalColumn: "OrderDetailId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Feedback_Product_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Product",
                        principalColumn: "ProductId");
                    table.ForeignKey(
                        name: "FK_Feedback_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "FeedbackMedia",
                columns: table => new
                {
                    FeedbackMediaId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FeedbackId = table.Column<long>(type: "bigint", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackMedia", x => x.FeedbackMediaId);
                    table.ForeignKey(
                        name: "FK_FeedbackMedia_Feedback_FeedbackId",
                        column: x => x.FeedbackId,
                        principalTable: "Feedback",
                        principalColumn: "FeedbackId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransactionCoin",
                columns: table => new
                {
                    TransactionCoinId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    OrderId = table.Column<long>(type: "bigint", nullable: true),
                    FeedbackId = table.Column<long>(type: "bigint", nullable: true),
                    TransactionCoinTypeId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<long>(type: "bigint", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateCreate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionCoin", x => x.TransactionCoinId);
                    table.ForeignKey(
                        name: "FK_TransactionCoin_Feedback_FeedbackId",
                        column: x => x.FeedbackId,
                        principalTable: "Feedback",
                        principalColumn: "FeedbackId");
                    table.ForeignKey(
                        name: "FK_TransactionCoin_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Order",
                        principalColumn: "OrderId");
                    table.ForeignKey(
                        name: "FK_TransactionCoin_TransactionCoinType_TransactionCoinTypeId",
                        column: x => x.TransactionCoinTypeId,
                        principalTable: "TransactionCoinType",
                        principalColumn: "TransactionCoinTypeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransactionCoin_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Bank",
                columns: new[] { "BankId", "BankCode", "BankName", "isActivate" },
                values: new object[,]
                {
                    { 458761L, "HSBC", "TNHH MTV HSBC Việt Nam (HSBC)", true },
                    { 970403L, "STB", "Sacombank (STB)", true },
                    { 970405L, "VBA", "Nông nghiệp và Phát triển nông thôn (VBA)", true },
                    { 970407L, "TCB", "Kỹ Thương (TCB)", true },
                    { 970415L, "VIETINBANK", "Công Thương Việt Nam (VIETINBANK)", true },
                    { 970418L, "BIDV", "Đầu tư và phát triển (BIDV)", true },
                    { 970422L, "MB", "Quân đội (MB)", true },
                    { 970423L, "TPB", "Tiên Phong (TPB)", true },
                    { 970426L, "MSB", "Hàng hải (MSB)", true },
                    { 970432L, "VPB", "Việt Nam Thinh Vượng (VPB)", true },
                    { 970436L, "VCB", "Ngoại thương Việt Nam (VCB)", true },
                    { 970441L, "VIB", "Quốc tế (VIB)", true },
                    { 970443L, "SHB", "Sài Gòn Hà Nội (SHB)", true },
                    { 970449L, "LPB", "Bưu điện Liên Việt (LPB)", true }
                });

            migrationBuilder.InsertData(
                table: "BusinessFee",
                columns: new[] { "BusinessFeeId", "EndDate", "Fee", "StartDate" },
                values: new object[] { 1L, null, 5L, new DateTime(2023, 11, 25, 22, 59, 50, 343, DateTimeKind.Local).AddTicks(2142) });

            migrationBuilder.InsertData(
                table: "Category",
                columns: new[] { "CategoryId", "CategoryName" },
                values: new object[,]
                {
                    { 1L, "Mạng xã hội" },
                    { 2L, "Giáo dục" },
                    { 3L, "Trò chơi" },
                    { 4L, "VPS" },
                    { 5L, "Khác" }
                });

            migrationBuilder.InsertData(
                table: "CouponType",
                columns: new[] { "CouponTypeId", "Name" },
                values: new object[,]
                {
                    { 1L, "For all products (Admin)" },
                    { 2L, "For all shop's product" },
                    { 3L, "For specific products" }
                });

            migrationBuilder.InsertData(
                table: "FeedbackBenefit",
                columns: new[] { "FeedbackBenefitId", "Coin", "EndDate", "StartDate" },
                values: new object[] { 1, 200, null, new DateTime(2023, 11, 25, 22, 59, 50, 343, DateTimeKind.Local).AddTicks(2166) });

            migrationBuilder.InsertData(
                table: "OrderStatus",
                columns: new[] { "OrderStatusId", "Name" },
                values: new object[,]
                {
                    { 1L, "Wait for customer confirmation" },
                    { 2L, "Confirmed" },
                    { 3L, "Complaint" },
                    { 4L, "Seller refunded" },
                    { 5L, "Dispute" },
                    { 6L, "Reject Complaint" },
                    { 7L, "Seller violates" }
                });

            migrationBuilder.InsertData(
                table: "ProductStatus",
                columns: new[] { "ProductStatusId", "ProductStatusName" },
                values: new object[,]
                {
                    { 1L, "Active" },
                    { 2L, "Ban" },
                    { 3L, "Remove" },
                    { 4L, "Hide" }
                });

            migrationBuilder.InsertData(
                table: "ReasonReportProduct",
                columns: new[] { "ReasonReportProductId", "EnExplanation", "EnName", "ViExplanation", "ViName" },
                values: new object[,]
                {
                    { 1, "", "", "Sản phẩm bị cấm theo quy định của Pháp luật hoặc nằm trong danh mục bị hạn chế trên FuDigitalHub", "Sản phẩm bị cấm buôn bán (động vật hoang dã, 18+,...)" },
                    { 2, "", "", "Hình ảnh sản phẩm chưa rõ ràng (quá nhỏ, không nhìn thấy toàn bộ sản phẩm hoặc bị che mờ các thông tin cần thiết)", "Hình ảnh sản phẩm không rõ ràng" },
                    { 3, "", "", "Sản phẩm có hình ảnh, nội dung phản cảm hoặc có thể gây phản cảm", "Sản phẩm có hình ảnh, nội dung phản cảm hoặc có thể gây phản cảm" },
                    { 4, "", "", "Thông tin sản phẩm có chứa yêu cầu bất hợp pháp: đặt cọc, chuyển khoản, gửi lại tiền thừa trong gói hàng, hoặc giao dịch ngoài DigitabFuHub, v.v...", "Sản phẩm có dấu hiệu lừa đảo" },
                    { 5, "", "", "Tên sản phẩm hoàn toàn khác với hình ảnh. Người bán cần sửa lại tên phù hợp với hình ảnh sản phẩm.", "Tên sản phẩm (Name) không phù hợp với hình ảnh sản phẩm" },
                    { 6, "", "", "Người bán có dấu hiệu tạo đơn ảo để tăng số lượng đơn hàng và đánh giá", "Sản phẩm có dấu hiệu tăng đơn ảo" },
                    { 7, "", "", "Sản phẩm vi phạm quy định khác của DigitabFuHub", "Khác" }
                });

            migrationBuilder.InsertData(
                table: "ReportProductStatus",
                columns: new[] { "ReportProductId", "Name" },
                values: new object[,]
                {
                    { 1, "verifying" },
                    { 2, "reject" },
                    { 3, "accept" }
                });

            migrationBuilder.InsertData(
                table: "Role",
                columns: new[] { "RoleId", "RoleName" },
                values: new object[,]
                {
                    { 1L, "Admin" },
                    { 2L, "Customer" },
                    { 3L, "Seller" }
                });

            migrationBuilder.InsertData(
                table: "TransactionCoinType",
                columns: new[] { "TransactionCoinTypeId", "Name" },
                values: new object[,]
                {
                    { 1, "Get coin" },
                    { 2, "Use coin" },
                    { 3, "Refund" }
                });

            migrationBuilder.InsertData(
                table: "TransactionInternalType",
                columns: new[] { "TransactionInternalTypeId", "Name" },
                values: new object[,]
                {
                    { 1L, "Payment" },
                    { 2L, "Receive payment" },
                    { 3L, "Receive refund" },
                    { 4L, "Profit" }
                });

            migrationBuilder.InsertData(
                table: "WithdrawTransactionStatus",
                columns: new[] { "WithdrawTransactionStatusId", "Name" },
                values: new object[,]
                {
                    { 1L, "In processing" },
                    { 2L, "Paid" },
                    { 3L, "Reject" },
                    { 4L, "Cancel" }
                });

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "UserId", "AccountBalance", "Avatar", "Coin", "Email", "Fullname", "IsChangeUsername", "IsConfirm", "IsOnline", "LastTimeOnline", "Password", "RoleId", "SignInGoogle", "Status", "TwoFactorAuthentication", "Username" },
                values: new object[] { 1L, 0L, "", 0L, "", "Admin", false, true, false, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "123", 1L, false, true, false, "admin" });

            migrationBuilder.CreateIndex(
                name: "IX_AccessToken_UserId",
                table: "AccessToken",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetInformation_OrderDetailId",
                table: "AssetInformation",
                column: "OrderDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetInformation_ProductVariantId",
                table: "AssetInformation",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_Cart_ShopId",
                table: "Cart",
                column: "ShopId");

            migrationBuilder.CreateIndex(
                name: "IX_Cart_UserId",
                table: "Cart",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CartDetail_CartId",
                table: "CartDetail",
                column: "CartId");

            migrationBuilder.CreateIndex(
                name: "IX_CartDetail_ProductVariantId",
                table: "CartDetail",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_Coupon_CouponTypeId",
                table: "Coupon",
                column: "CouponTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Coupon_ShopId",
                table: "Coupon",
                column: "ShopId");

            migrationBuilder.CreateIndex(
                name: "IX_CouponProduct_ProductId",
                table: "CouponProduct",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_DepositTransaction_UserId",
                table: "DepositTransaction",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_FeedbackBenefitId",
                table: "Feedback",
                column: "FeedbackBenefitId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_OrderDetailId",
                table: "Feedback",
                column: "OrderDetailId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_ProductId",
                table: "Feedback",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedback_UserId",
                table: "Feedback",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackMedia_FeedbackId",
                table: "FeedbackMedia",
                column: "FeedbackId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryOrderStatus_OrderStatusId",
                table: "HistoryOrderStatus",
                column: "OrderStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ConversationId",
                table: "Messages",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_UserId",
                table: "Messages",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_UserId",
                table: "Notification",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_BusinessFeeId",
                table: "Order",
                column: "BusinessFeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_ConversationId",
                table: "Order",
                column: "ConversationId",
                unique: true,
                filter: "[ConversationId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Order_OrderStatusId",
                table: "Order",
                column: "OrderStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_ShopId",
                table: "Order",
                column: "ShopId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_UserId",
                table: "Order",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderCoupon_CouponId",
                table: "OrderCoupon",
                column: "CouponId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetail_OrderId",
                table: "OrderDetail",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderDetail_ProductVariantId",
                table: "OrderDetail",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_Product_CategoryId",
                table: "Product",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Product_ProductStatusId",
                table: "Product",
                column: "ProductStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Product_ShopId",
                table: "Product",
                column: "ShopId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductMedia_ProductId",
                table: "ProductMedia",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariant_ProductId",
                table: "ProductVariant",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_UserId",
                table: "RefreshToken",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportProduct_ProductId",
                table: "ReportProduct",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportProduct_ReasonReportProductId",
                table: "ReportProduct",
                column: "ReasonReportProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportProduct_ReportProductStatusId",
                table: "ReportProduct",
                column: "ReportProductStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportProduct_UserId",
                table: "ReportProduct",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tag_ProductId",
                table: "Tag",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionCoin_FeedbackId",
                table: "TransactionCoin",
                column: "FeedbackId",
                unique: true,
                filter: "[FeedbackId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionCoin_OrderId",
                table: "TransactionCoin",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionCoin_TransactionCoinTypeId",
                table: "TransactionCoin",
                column: "TransactionCoinTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionCoin_UserId",
                table: "TransactionCoin",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionInternal_OrderId",
                table: "TransactionInternal",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionInternal_TransactionInternalTypeId",
                table: "TransactionInternal",
                column: "TransactionInternalTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionInternal_UserId",
                table: "TransactionInternal",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TwoFactorAuthentication_UserId",
                table: "TwoFactorAuthentication",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_User_RoleId",
                table: "User",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBank_BankId",
                table: "UserBank",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_UserBank_UserId",
                table: "UserBank",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserConversation_ConversationId",
                table: "UserConversation",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserConversation_UserId",
                table: "UserConversation",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WishList_ProductId",
                table: "WishList",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_WithdrawTransaction_UserBankId",
                table: "WithdrawTransaction",
                column: "UserBankId");

            migrationBuilder.CreateIndex(
                name: "IX_WithdrawTransaction_UserId",
                table: "WithdrawTransaction",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WithdrawTransaction_WithdrawTransactionStatusId",
                table: "WithdrawTransaction",
                column: "WithdrawTransactionStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_WithdrawTransactionBill_WithdrawTransactionId",
                table: "WithdrawTransactionBill",
                column: "WithdrawTransactionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessToken");

            migrationBuilder.DropTable(
                name: "AssetInformation");

            migrationBuilder.DropTable(
                name: "CartDetail");

            migrationBuilder.DropTable(
                name: "CouponProduct");

            migrationBuilder.DropTable(
                name: "DepositTransaction");

            migrationBuilder.DropTable(
                name: "FeedbackMedia");

            migrationBuilder.DropTable(
                name: "HistoryOrderStatus");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Notification");

            migrationBuilder.DropTable(
                name: "OrderCoupon");

            migrationBuilder.DropTable(
                name: "ProductMedia");

            migrationBuilder.DropTable(
                name: "RefreshToken");

            migrationBuilder.DropTable(
                name: "ReportProduct");

            migrationBuilder.DropTable(
                name: "Tag");

            migrationBuilder.DropTable(
                name: "TransactionCoin");

            migrationBuilder.DropTable(
                name: "TransactionInternal");

            migrationBuilder.DropTable(
                name: "TwoFactorAuthentication");

            migrationBuilder.DropTable(
                name: "UserConversation");

            migrationBuilder.DropTable(
                name: "WishList");

            migrationBuilder.DropTable(
                name: "WithdrawTransactionBill");

            migrationBuilder.DropTable(
                name: "Cart");

            migrationBuilder.DropTable(
                name: "Coupon");

            migrationBuilder.DropTable(
                name: "ReasonReportProduct");

            migrationBuilder.DropTable(
                name: "ReportProductStatus");

            migrationBuilder.DropTable(
                name: "Feedback");

            migrationBuilder.DropTable(
                name: "TransactionCoinType");

            migrationBuilder.DropTable(
                name: "TransactionInternalType");

            migrationBuilder.DropTable(
                name: "WithdrawTransaction");

            migrationBuilder.DropTable(
                name: "CouponType");

            migrationBuilder.DropTable(
                name: "FeedbackBenefit");

            migrationBuilder.DropTable(
                name: "OrderDetail");

            migrationBuilder.DropTable(
                name: "UserBank");

            migrationBuilder.DropTable(
                name: "WithdrawTransactionStatus");

            migrationBuilder.DropTable(
                name: "Order");

            migrationBuilder.DropTable(
                name: "ProductVariant");

            migrationBuilder.DropTable(
                name: "Bank");

            migrationBuilder.DropTable(
                name: "BusinessFee");

            migrationBuilder.DropTable(
                name: "Conversations");

            migrationBuilder.DropTable(
                name: "OrderStatus");

            migrationBuilder.DropTable(
                name: "Product");

            migrationBuilder.DropTable(
                name: "Category");

            migrationBuilder.DropTable(
                name: "ProductStatus");

            migrationBuilder.DropTable(
                name: "Shop");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Role");
        }
    }
}
