﻿using AutoMapper;
using Azure;
using BusinessObject.Entities;
using Comons;
using DataAccess.IRepositories;
using DataAccess.Repositories;
using DigitalFUHubApi.Comons;
using DigitalFUHubApi.Services;
using DTOs.Coupon;
using DTOs.Seller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System.Globalization;
using System.Reflection.Metadata;

namespace DigitalFUHubApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CouponsController : ControllerBase
	{
		private readonly IConfiguration _configuration;
		private readonly IProductRepository _productRepository;
		private readonly StorageService _storageService;
		private readonly IShopRepository _shopRepository;
		private readonly IUserRepository _userRepository;
		private readonly IRoleRepository _roleRepository;
		private readonly IOrderRepository _orderRepository;
		private readonly ICouponRepository _couponRepository;
		private readonly JwtTokenService _jwtTokenService;
		private readonly IMapper _mapper;

		public CouponsController(IConfiguration configuration, IProductRepository productRepository, StorageService storageService, IShopRepository shopRepository, IUserRepository userRepository, IRoleRepository roleRepository, IOrderRepository orderRepository, ICouponRepository couponRepository, JwtTokenService jwtTokenService, IMapper mapper)
		{
			_configuration = configuration;
			_productRepository = productRepository;
			_storageService = storageService;
			_shopRepository = shopRepository;
			_userRepository = userRepository;
			_roleRepository = roleRepository;
			_orderRepository = orderRepository;
			_couponRepository = couponRepository;
			_jwtTokenService = jwtTokenService;
			_mapper = mapper;
		}

		#region Get coupon public (customer)
		[HttpGet("GetCouponPublic")]
		public IActionResult GetCouponPublic(long shopId)
		{
			ResponseData response = new ResponseData();
			try
			{
				if (shopId == 0)
				{
					response.Status.Ok = false;
					response.Status.Message = "Invalid";
					response.Status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
					return Ok(response);
				}

				List<CouponResponseDTO> coupons = _mapper.Map<List<CouponResponseDTO>>(_couponRepository.GetCouponPublic(shopId));
				response.Status.ResponseCode = Constants.RESPONSE_CODE_SUCCESS;
				response.Status.Message = "Success";
				response.Status.Ok = true;
				response.Result = coupons;

			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}

			return Ok(response);
		}
		#endregion


		#region Get coupon by code
		[HttpGet("GetCouponByCode")]
		public IActionResult GetCouponByCode(string couponCode)
		{
			ResponseData response = new ResponseData();
			try
			{
				if (string.IsNullOrEmpty(couponCode))
				{
					response.Status.Ok = false;
					response.Status.Message = "Invalid";
					response.Status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
					return Ok(response);
				}

				CouponResponseDTO coupon = _mapper.Map<CouponResponseDTO>(_couponRepository.GetCouponByCode(couponCode));
				response.Status.ResponseCode = Constants.RESPONSE_CODE_SUCCESS;
				response.Status.Message = "Success";
				response.Status.Ok = true;
				response.Result = coupon;

			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}

			return Ok(response);
		}
		#endregion

		#region Get coupon of seller by Id
		[Authorize("Seller")]
		[HttpGet("{userId}/{couponId}")]
		public IActionResult GetCouponById(long userId, long couponId)
		{
			Coupon? coupon = _couponRepository.GetCoupon(couponId, userId);
			if (coupon == null)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "NOT FOUND", false, new()));
			}
			return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "SUCCESS", true, _mapper.Map<SellerCouponResponseDTO>(coupon)));
		}
		#endregion

		#region Get list coupons

		[Authorize("Seller")]
		[HttpPost("List/Seller")]
		public IActionResult GetListCouponsSeller(SellerCouponRequestDTO request)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "INVALID", false, new()));
				}

				string formatDate = "M/d/yyyy";
				CultureInfo provider = CultureInfo.InvariantCulture;
				DateTime? startDate = string.IsNullOrEmpty(request.StartDate) ? null : DateTime.ParseExact(request.StartDate.Trim(), formatDate, provider);
				DateTime? endDate = string.IsNullOrEmpty(request.EndDate) ? null : DateTime.ParseExact(request.EndDate.Trim(), formatDate, provider);

				List<Coupon> coupons = _couponRepository.GetListCouponsByShop(request.UserId, request.CouponCode.Trim(), startDate, endDate, request.Status);

				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "SUCCESS", true,
					coupons.Select(x => new SellerCouponResponseDTO
					{
						CouponId = x.CouponId,
						CouponCode = x.CouponCode,
						StartDate = x.StartDate,
						EndDate = x.EndDate,
						Quantity = x.Quantity,
						PriceDiscount = x.PriceDiscount,
						MinTotalOrderValue = x.MinTotalOrderValue,
						IsPublic = x.IsPublic
					}).ToList()));
			}
			catch (Exception)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "FAIL", false, new()));
			}
		}
		#endregion

		#region add coupon
		[Authorize("Seller")]
		[HttpPost("Add")]
		public IActionResult AddCoupon(SellerAddCouponRequestDTO request)
		{
			try
			{
				if (!ModelState.IsValid
					|| string.IsNullOrWhiteSpace(request.CouponCode)
					|| string.IsNullOrWhiteSpace(request.CouponName))
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "INVALID", false, new()));
				}
				DateTime startDT = DateTime.Parse(request.StartDate);
				DateTime endDT = DateTime.Parse(request.EndDate);
				Coupon coupon = new Coupon
				{
					IsPublic = request.IsPublic,
					IsActive = true,
					StartDate = startDT.AddSeconds(-startDT.Second),
					EndDate = endDT.AddSeconds(-endDT.Second),
					CouponName = request.CouponName,
					CouponCode = request.CouponCode,
					PriceDiscount = request.PriceDiscount,
					MinTotalOrderValue = request.MinTotalOrderValue,
					Quantity = request.Quantity,
					ShopId = request.UserId
				};
				_couponRepository.AddCoupon(coupon);

				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "SUCCESS", true, new()));
			}
			catch (Exception)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_FAILD, "INVALID", true, new()));
			}
		}
		#endregion

		#region Edit coupon
		[Authorize("Seller")]
		[HttpPost("Edit")]
		public IActionResult UpdateCoupon(SellerEditCouponRequestDTO request)
		{
			try
			{
				if (!ModelState.IsValid
					|| string.IsNullOrWhiteSpace(request.CouponCode)
					|| string.IsNullOrWhiteSpace(request.CouponName)
					)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "INVALID", false, new()));
				}
				DateTime startDT = DateTime.Parse(request.StartDate);
				DateTime endDT = DateTime.Parse(request.EndDate);
				Coupon coupon = new Coupon
				{
					CouponId = request.CouponId,
					IsPublic = request.IsPublic,
					StartDate = startDT,
					EndDate = endDT,
					CouponName = request.CouponName.Trim(),
					CouponCode = request.CouponCode.Trim(),
					PriceDiscount = request.PriceDiscount,
					MinTotalOrderValue = request.MinTotalOrderValue,
					Quantity = request.Quantity,
					ShopId = request.UserId
				};
				_couponRepository.UpdateCoupon(coupon);

				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "SUCCESS", true, new()));
			}
			catch (Exception)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_FAILD, "INVALID", true, new()));
			}
		}
		#endregion

		#region update status coupon
		[Authorize("Seller")]
		[HttpPost("Edit/Status")]
		public IActionResult UpdateStatusCoupon(SellerUpdateStatusCouponDTO request)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "INVALID", false, new()));
				}

				Coupon? coupon = _couponRepository.GetCoupon(request.CouponId, request.UserId);
				if (coupon == null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "INVALID", false, new()));
				}
				coupon.IsPublic = request.IsPublic;
				_couponRepository.UpdateStatusCoupon(coupon);

				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "SUCCESS", true, new()));
			}
			catch (Exception)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_FAILD, "INVALID", true, new()));
			}
		}
		#endregion


		#region Remove coupon
		[Authorize("Seller")]
		[HttpPost("Remove")]
		public IActionResult UpdateStatusCoupon(SellerRemoveCouponRequestDTO request)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "INVALID", false, new()));
				}

				Coupon? coupon = _couponRepository.GetCoupon(request.CouponId, request.UserId);
				if (coupon == null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "INVALID", false, new()));
				}
				coupon.IsActive = false;
				_couponRepository.UpdateStatusCoupon(coupon);

				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "SUCCESS", true, new()));
			}
			catch (Exception e)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_FAILD, e.Message, false, new()));
			}
		}
		#endregion


		[Authorize("Seller")]
		[HttpGet("IsExistCouponCode/{userId}/{couponCode}")]
		public IActionResult CheckCouponCodeExist(long userId, string couponCode)
		{
			if (string.IsNullOrWhiteSpace(couponCode))
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "INVALID", false, new()));
			}

			bool result = _couponRepository.IsExistCouponCode(userId, couponCode.Trim());
			return Ok(new ResponseData(result ? Constants.RESPONSE_CODE_FAILD : Constants.RESPONSE_CODE_SUCCESS,
				result ? "INVALID" : "SUCCESS",
				!result,
				new()));
		}
	}
}
