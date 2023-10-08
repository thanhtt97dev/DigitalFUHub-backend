﻿using AutoMapper;
using DataAccess.DAOs;
using DTOs;
using DigitalFUHubApi.Comons;
using BusinessObject.Entities;
using DTOs.Bank;
using DTOs.User;
using DTOs.Notification;
using DTOs.Chat;
using DTOs.Product;
using DTOs.Tag;
using DTOs.Cart;
using DTOs.Admin;
using DTOs.Order;

namespace DigitalFUHubApi.Comons
{
	public class AutoMapperProfile : Profile
	{
		public AutoMapperProfile()
		{
			CreateMap<User, UserResponeDTO>()
				.ForMember(des => des.RoleName, act => act.MapFrom(src => src.Role.RoleName)).ReverseMap();
			CreateMap<User, UserSignInResponseDTO>()
				.ForMember(des => des.RoleName, act => act.MapFrom(src => src.Role.RoleName)).ReverseMap();
			CreateMap<User, UserUpdateRequestDTO>().ReverseMap();
			CreateMap<Role, RoleDTO>().ReverseMap();
			CreateMap<Notification, NotificationRespone>().ReverseMap();
			CreateMap<DepositTransaction, CreateTransactionRequestDTO>().ReverseMap();
			CreateMap<WithdrawTransaction, CreateTransactionRequestDTO>().ReverseMap();
			CreateMap<Bank, BankResponeDTO>().ReverseMap();
			CreateMap<UserBank, BankAccountResponeDTO>()
				.ForMember(des => des.BankName, act => act.MapFrom(src => src.Bank.BankName))
				.ForMember(des => des.CreditAccount, act => act.MapFrom(src => Util.HideCharacters(src.CreditAccount, 5)))
				.ReverseMap();
			CreateMap<Message, MessageResponseDTO>().ReverseMap();
			CreateMap<WithdrawTransactionBill, WithdrawTransactionBillDTO>().ReverseMap();
			CreateMap<WithdrawTransaction, HistoryWithdrawResponsetDTO>()
				.ForMember(des => des.BankName, act => act.MapFrom(src => src.UserBank.Bank.BankName))
				.ForMember(des => des.CreditAccountName, act => act.MapFrom(src => src.UserBank.CreditAccountName))
				.ForMember(des => des.Email, act => act.MapFrom(src => src.User.Email))
				.ForMember(des => des.UserId, act => act.MapFrom(src => src.User.UserId))
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
				.ForMember(des => des.SellerId, act => act.MapFrom(src => src.ProductVariant.Product.Shop.UserId))
				.ForMember(des => des.ShopName, act => act.MapFrom(src => src.ProductVariant.Product.Shop.ShopName))
				.ReverseMap();
			CreateMap<OrderCoupon, OrderDetailOrderCouponsDTO>()
				.ReverseMap();
			CreateMap<Order, OrderDetailResponseDTO>()
				.ForMember(des => des.CustomerId, act => act.MapFrom(src => src.User.UserId))
				.ForMember(des => des.CustomerEmail, act => act.MapFrom(src => src.User.Email))
				.ForMember(des => des.ProductVariantName, act => act.MapFrom(src => src.ProductVariant.Name))
				.ForMember(des => des.ProductId, act => act.MapFrom(src => src.ProductVariant.ProductId))
				.ForMember(des => des.ProductName, act => act.MapFrom(src => src.ProductVariant.Product.ProductName))
				.ForMember(des => des.ProductThumbnail, act => act.MapFrom(src => src.ProductVariant.Product.Thumbnail))
				.ForMember(des => des.ShopId, act => act.MapFrom(src => src.ProductVariant.Product.Shop.UserId))
				.ForMember(des => des.ShopName, act => act.MapFrom(src => src.ProductVariant.Product.Shop.ShopName))
				.ForMember(des => des.ProductCategoryId, act => act.MapFrom(src => src.ProductVariant.Product.Category.CategoryId))
				.ForMember(des => des.ProductCategoryName, act => act.MapFrom(src => src.ProductVariant.Product.Category.CategoryName))
				.ForMember(des => des.FeedbackRate, opt => opt.MapFrom((src, dest) =>
				{
					if (src.Feedback == null) return 0;
					return src.Feedback.Rate;
				}))
				.ForMember(des => des.BusinessFeeValue, act => act.MapFrom(src => src.BusinessFee.Fee))
				.ForMember(des => des.AssetInformations, act => act.MapFrom(src => src.AssetInformations.Select(x => x.Asset).ToList()))
				.ForMember(des => des.ProductMedias, act => act.MapFrom(src => src.ProductVariant.Product.ProductMedias.Select(x => x.Url).ToList()))
				.ReverseMap();


			CreateMap<Transaction, HistoryTransactionInternalResponseDTO>()
				.ForMember(des => des.Email, act => act.MapFrom(src => src.User.Email))
				.ReverseMap();
			/*
			CreateMap<Coupon, CouponRequestAddOrderDTO>()
				.ForMember(des => des.CouponCode, act => act.MapFrom(src => src.CouponCode))
				.ReverseMap();
			*/
			
			CreateMap<Transaction, HistoryTransactionInternalResponseDTO>()
				.ForMember(des => des.Email, act => act.MapFrom(src => src.User.Email))
				.ReverseMap();

			CreateMap<Product, ProductResponseDTO>().ReverseMap(); ;
			CreateMap<ProductVariant, ProductVariantResponseDTO>().ReverseMap(); ;
			CreateMap<ProductMedia, ProductMediaResponseDTO>().ReverseMap(); ;
			CreateMap<Tag, TagResponseDTO>().ReverseMap(); ;
			CreateMap<AssetInformation, AssetInformationResponseDTO>().ReverseMap();
			CreateMap<Cart, CartDTO>().ReverseMap();
			CreateMap<Order, AddOrderRequestDTO>().ReverseMap();


		}
	}
}
