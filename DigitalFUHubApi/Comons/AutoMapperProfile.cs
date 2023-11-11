using AutoMapper;
using DataAccess.DAOs;
using DTOs;
using DigitalFUHubApi.Comons;
using BusinessObject.Entities;
using DTOs.Bank;
using DTOs.User;
using DTOs.Notification;
using DTOs.Product;
using DTOs.Tag;
using DTOs.Cart;
using DTOs.Admin;
using DTOs.Order;
using DTOs.Coupon;
using DTOs.Conversation;
using Comons;
using DTOs.Seller;
using DTOs.Feedback;
using DTOs.WishList;
using Google.Apis.Util;
using DTOs.TransactionCoin;
using System.Globalization;

namespace DigitalFUHubApi.Comons
{
	public class AutoMapperProfile : Profile
	{
		private string MapOrderStatusToString(long orderStatus)
		{
			switch (orderStatus)
			{
				case Constants.ORDER_STATUS_COMPLAINT:
					return "Đang khiếu nại";
				case Constants.ORDER_STATUS_CONFIRMED:
					return "Đã xác nhận";
				case Constants.ORDER_STATUS_DISPUTE:
					return "Đang tranh chấp";
				case Constants.ORDER_STATUS_REJECT_COMPLAINT:
					return "Từ chối khiếu nại";
				case Constants.ORDER_STATUS_SELLER_REFUNDED:
					return "Người bán hoàn trả tiền";
				case Constants.ORDER_STATUS_SELLER_VIOLATES:
					return "Người bán vi phạm";
				case Constants.ORDER_STATUS_WAIT_CONFIRMATION:
					return "Đợi người mua xác nhận";
				default:
					return "";
			}
		}
		public AutoMapperProfile()
		{
			CreateMap<User, UserResponeDTO>()
				.ForMember(des => des.RoleName, act => act.MapFrom(src => src.Role.RoleName)).ReverseMap();
			CreateMap<User, UserSignInResponseDTO>()
				.ForMember(des => des.RoleName, act => act.MapFrom(src => src.Role.RoleName)).ReverseMap();
			CreateMap<User, UserUpdateRequestDTO>().ReverseMap();
			CreateMap<User, UserOnlineStatusResponseDTO>().ReverseMap();
			CreateMap<Role, RoleDTO>().ReverseMap();
			CreateMap<Notification, NotificationRespone>().ReverseMap();
			CreateMap<DepositTransaction, CreateTransactionRequestDTO>().ReverseMap();
			CreateMap<WithdrawTransaction, CreateTransactionRequestDTO>().ReverseMap();
			CreateMap<Bank, BankResponeDTO>().ReverseMap();
			CreateMap<UserBank, BankAccountResponeDTO>()
				.ForMember(des => des.BankName, act => act.MapFrom(src => src.Bank.BankName))
				.ForMember(des => des.CreditAccount, act => act.MapFrom(src => Util.HideCharacters(src.CreditAccount, 5)))
				.ReverseMap();
			CreateMap<Message, MessageConversationResponseDTO>()
				.ForMember(des => des.Avatar, act => act.MapFrom(src => src.User.Avatar)).ReverseMap();
			CreateMap<WithdrawTransactionBill, WithdrawTransactionBillDTO>().ReverseMap();
			CreateMap<WithdrawTransaction, HistoryWithdrawResponsetDTO>()
				.ForMember(des => des.BankName, act => act.MapFrom(src => src.UserBank.Bank.BankName))
				.ForMember(des => des.CreditAccountName, act => act.MapFrom(src => src.UserBank.CreditAccountName))
				.ForMember(des => des.Email, act => act.MapFrom(src => src.User.Email))
				.ForMember(des => des.UserId, act => act.MapFrom(src => src.UserId))
				//.ForMember(des => des.CreditAccount, atc => atc.MapFrom(src => Util.HideCharacters(src.UserBank.CreditAccount, 5)))
				.ForMember(des => des.CreditAccount, atc => atc.MapFrom(src => src.UserBank.CreditAccount))
				.ForMember(des => des.BankCode, atc => atc.MapFrom(src => src.UserBank.Bank.BankCode))
				.ReverseMap();
			CreateMap<DepositTransaction, HistoryDepositResponeDTO>()
				.ForMember(des => des.Email, act => act.MapFrom(src => src.User.Email))
				.ReverseMap();
			CreateMap<Order, OrdersResponseDTO>()
				.ForMember(des => des.CustomerId, act => act.MapFrom(src => src.User.UserId))
				.ForMember(des => des.CustomerEmail, act => act.MapFrom(src => src.User.Email))
				.ForMember(des => des.SellerId, act => act.MapFrom(src => src.Shop.UserId))
				.ForMember(des => des.ShopName, act => act.MapFrom(src => src.Shop.ShopName))
				.ReverseMap();
			CreateMap<Order, SellerOrderResponseDTO>()
				.ForMember(des => des.Username, act => act.MapFrom(src => src.User.Username))
				.ReverseMap();
			CreateMap<OrderCoupon, OrderDetailInfoResponseDTO>()
				.ReverseMap();
			CreateMap<OrderDetail, OrderDetailInfoResponseDTO>()
				.ForMember(des => des.ProductVariantName, act => act.MapFrom(src => src.ProductVariant.Name))
				.ForMember(des => des.ProductId, act => act.MapFrom(src => src.ProductVariant.ProductId))
				.ForMember(des => des.ProductName, act => act.MapFrom(src => src.ProductVariant.Product.ProductName))
				.ForMember(des => des.ProductThumbnail, act => act.MapFrom(src => src.ProductVariant.Product.Thumbnail))
				.ForMember(des => des.FeedbackRate, opt => opt.MapFrom((src, dest) =>
				{
					if (src.Feedback == null) return 0;
					return src.Feedback.Rate;
				}))
				.ForMember(des => des.FeedbackBenefit, opt => opt.MapFrom((src, dest) =>
				{
					if (src.Feedback == null) return 0;
					return src.Feedback.FeedbackBenefit.Coin;
				}))
				.ReverseMap();

			CreateMap<AssetInformation, AssetInfomationOrderDetailResponeDTO>()
				.ForMember(des => des.Data, act => act.MapFrom(src => src.Asset))
				.ReverseMap();

			CreateMap<TransactionCoin, TransactionCoinOrderDetailResponseDTO>()
				.ReverseMap();

			CreateMap<TransactionInternal, TransactionInternalOrderDetailResponseDTO>()
				.ReverseMap();

			CreateMap<HistoryOrderStatus, HistoryOrderStatusOrderDetailDTO>()
				.ReverseMap();

			CreateMap<OrderDetail, OrderDetailInfoResponseDTO>()
				.ForMember(des => des.ProductVariantId, act => act.MapFrom(src => src.ProductVariant.ProductVariantId))
				.ForMember(des => des.ProductVariantName, act => act.MapFrom(src => src.ProductVariant.Name))
				.ForMember(des => des.ProductId, act => act.MapFrom(src => src.ProductVariant.Product.ProductId))
				.ForMember(des => des.ProductName, act => act.MapFrom(src => src.ProductVariant.Product.ProductName))
				.ForMember(des => des.ProductThumbnail, act => act.MapFrom(src => src.ProductVariant.Product.Thumbnail))
				.ForMember(des => des.FeedbackRate, opt => opt.MapFrom((src, dest) =>
				{
					if (src.Feedback == null) return 0;
					return src.Feedback.Rate;
				}))
				.ForMember(des => des.FeedbackBenefit, opt => opt.MapFrom((src, dest) =>
				{
					if (src.Feedback == null) return 0;
					return src.Feedback.FeedbackBenefit.Coin;
				}))
				.ForMember(des => des.AssetInfomations, act => act.MapFrom(src => src.AssetInformations))
				.ReverseMap();

			CreateMap<Order, OrderInfoResponseDTO>()
				.ForMember(des => des.CustomerId, act => act.MapFrom(src => src.User.UserId))
				.ForMember(des => des.CustomerEmail, act => act.MapFrom(src => src.User.Email))
				.ForMember(des => des.CustomerAvatar, act => act.MapFrom(src => src.User.Avatar))
				.ForMember(des => des.ShopId, act => act.MapFrom(src => src.Shop.UserId))
				.ForMember(des => des.ShopName, act => act.MapFrom(src => src.Shop.ShopName))
				.ForMember(des => des.BusinessFeeId, act => act.MapFrom(src => src.BusinessFee.BusinessFeeId))
				.ForMember(des => des.BusinessFeeValue, act => act.MapFrom(src => src.BusinessFee.Fee))
				.ReverseMap();

			CreateMap<CartDetail, UserCartProductResponseDTO>()
				.ForMember(des => des.ProductVariantId, act => act.MapFrom(src => src.ProductVariant.ProductVariantId))
				.ForMember(des => des.ProductVariantName, act => act.MapFrom(src => src.ProductVariant.Name))
				.ForMember(des => des.ProductVariantPrice, act => act.MapFrom(src => src.ProductVariant.Price))
				.ForMember(des => des.ProductVariantActivate, act => act.MapFrom(src => src.ProductVariant.isActivate))
				.ForMember(des => des.ProductId, act => act.MapFrom(src => src.ProductVariant.Product.ProductId))
				.ForMember(des => des.ProductName, act => act.MapFrom(src => src.ProductVariant.Product.ProductName))
				.ForMember(des => des.ProductDiscount, act => act.MapFrom(src => src.ProductVariant.Product.Discount))
				.ForMember(des => des.ProductThumbnail, act => act.MapFrom(src => src.ProductVariant.Product.Thumbnail))
				.ForMember(des => des.ProductActivate, act => act.MapFrom(src => src.ProductVariant.Product.ProductStatusId == Constants.PRODUCT_STATUS_ACTIVE))
				.ForMember(des => des.QuantityProductRemaining, act => act.MapFrom(src => src.ProductVariant.AssetInformations.Count()))
				.ReverseMap();
			CreateMap<Cart, UserCartResponseDTO>()
				.ForMember(des => des.ShopId, act => act.MapFrom(src => src.Shop.UserId))
				.ForMember(des => des.ShopName, act => act.MapFrom(src => src.Shop.ShopName))
				.ForMember(des => des.Products, act => act.MapFrom(src => src.CartDetails))
				.ReverseMap();
			CreateMap<TransactionInternal, HistoryTransactionInternalResponseDTO>()
				.ForMember(des => des.Email, act => act.MapFrom(src => src.User.Email))
				.ReverseMap();
			CreateMap<TransactionInternal, HistoryTransactionInternalResponseDTO>()
				.ForMember(des => des.Email, act => act.MapFrom(src => src.User.Email))
				.ReverseMap();
			CreateMap<User, UsersResponseDTO>()
				.ForMember(des => des.RoleName, act => act.MapFrom(src => src.Role.RoleName))
				.ReverseMap();
			CreateMap<OrderCoupon, OrderCouponResponseDTO>()
				.ForMember(des => des.CouponName, act => act.MapFrom(src => src.Coupon.CouponName))
				.ReverseMap();

			CreateMap<Product, ProductResponseDTO>().ReverseMap(); ;
			CreateMap<ProductVariant, ProductVariantResponseDTO>().ReverseMap(); ;
			CreateMap<ProductMedia, ProductMediaResponseDTO>().ReverseMap(); ;
			CreateMap<Tag, TagResponseDTO>().ReverseMap();
			CreateMap<AssetInformation, AssetInformationResponseDTO>().ReverseMap();
			CreateMap<Cart, CartDTO>().ReverseMap();
			CreateMap<Cart, UpdateCartRequestDTO>().ReverseMap();
			CreateMap<Order, AddOrderRequestDTO>().ReverseMap();
			CreateMap<Coupon, CouponResponseDTO>()
				.ForMember(des => des.productIds, act => act.MapFrom(src => src.CouponProducts != null ? src.CouponProducts.Select(x => x.ProductId).ToList() : new List<long>()))
				.ReverseMap();
			CreateMap<Coupon, SellerCouponResponseDTO>()
				.ForMember(des => des.ProductsApplied, act => act.MapFrom(src => src.CouponProducts.Select(x => new Product
				{
					ProductId = x.ProductId,
					ProductName = x.Product.ProductName,
					ProductStatusId = x.Product.ProductId,
					Thumbnail = x.Product.Thumbnail,
					NumberFeedback = x.Product.NumberFeedback,
					TotalRatingStar = x.Product.TotalRatingStar,
					Discount = x.Product.Discount,
					Description = x.Product.Description,
					UpdateDate = x.Product.UpdateDate,
					ShopId = x.Product.ShopId,
					CategoryId = x.Product.CategoryId,
				}).ToList()))
				.ReverseMap();
			CreateMap<Product, WishListProductResponseDTO>().ReverseMap();
			CreateMap<ProductVariant, WishListProductVariantResponseDTO>().ReverseMap();

			CreateMap<TransactionCoin, HistoryTransactionCoinResponseDTO>()
				.ForMember(des => des.Email, act => act.MapFrom(src => src.User.Email))
				.ReverseMap();

			CreateMap<Order, SellerFeedbackResponseDTO>()
				.ForMember(des => des.CustomerUsername, act => act.MapFrom(src => src.User.Username))
				.ForMember(des => des.CustomerAvatar, act => act.MapFrom(src => src.User.Avatar))
				.ForMember(des => des.Detail, act => act.MapFrom(src => src.OrderDetails))
				.ReverseMap();
			CreateMap<OrderDetail, SellerFeedbackDetailResponseDTO>()
				.ForMember(des => des.ProductName, act => act.MapFrom(src => src.ProductVariant.Product.ProductName))
				.ForMember(des => des.Thumbnail, act => act.MapFrom(src => src.ProductVariant.Product.Thumbnail))
				.ForMember(des => des.ProductVariantName, act => act.MapFrom(src => src.ProductVariant.Name))
				.ForMember(des => des.Content, act => act.MapFrom(src => src.Feedback == null ? "" : src.Feedback.Content))
				.ForMember(des => des.Rate, act => act.MapFrom(src => src.Feedback == null ? 0 : src.Feedback.Rate))
				.ForMember(des => des.FeedbackDate, act => act.MapFrom(src => src.Feedback == null ? new DateTime() : src.Feedback.UpdateDate))
				.ForMember(des => des.UrlImageFeedback, act => act.MapFrom(src => src.Feedback == null ? null : src.Feedback.FeedbackMedias))
				.ReverseMap();
			CreateMap<FeedbackMedia, string>()
				.ConvertUsing(r => r.Url);

			// products/admin/getProducts
			CreateMap<ProductVariant, GetProductsProductVariantDetailResponseDTO>()
				.ForMember(des => des.ProductVariantId, act => act.MapFrom(src => src.ProductVariantId))
				.ForMember(des => des.ProductVariantName, act => act.MapFrom(src => src.Name))
				.ForMember(des => des.ProductVariantPrice, act => act.MapFrom(src => src.Price))
				.ReverseMap();
			CreateMap<Product, GetProductsProductDetailResponseDTO>()
				.ForMember(des => des.ShopId, act => act.MapFrom(src => src.Shop.UserId))
				.ForMember(des => des.ShopName, act => act.MapFrom(src => src.Shop.ShopName))
				.ForMember(des => des.ProductId, act => act.MapFrom(src => src.ProductId))
				.ForMember(des => des.ProductName, act => act.MapFrom(src => src.ProductName))
				.ForMember(des => des.ProductThumbnail, act => act.MapFrom(src => src.Thumbnail))
				.ReverseMap();
			CreateMap<Order, SellerReportOrderResponseDTO>()
				.ForMember(des => des.Username, act => act.MapFrom(src => src.User.Username))
				.ForMember(des => des.TotalAmount, act => act.MapFrom(src => src.TotalAmount.ToString("{0:c}", CultureInfo.GetCultureInfo("vi-VN"))))
				.ForMember(des => des.TotalCouponDiscount, act => act.MapFrom(src => src.TotalCouponDiscount.ToString("{0:c}", CultureInfo.GetCultureInfo("vi-VN"))))
				.ForMember(des => des.BusinessFee, act => act.MapFrom(src => src.BusinessFee.Fee + "%"))
				.ForMember(des => des.Profit, act => act.MapFrom(src => ((src.TotalAmount - src.TotalCouponDiscount) - ((src.TotalAmount - src.TotalCouponDiscount) * src.BusinessFee.Fee / 100)).ToString("{0:c}", CultureInfo.GetCultureInfo("vi-VN"))))
				.ForMember(des => des.OrderStatus, act => act.MapFrom(src => MapOrderStatusToString(src.OrderStatusId)))
				.ReverseMap();
		}
	}
}
