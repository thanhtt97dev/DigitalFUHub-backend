using AutoMapper;
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

		[HttpGet("getCoupons")]
		public IActionResult GetCoupons(long shopId, string couponCode)
		{
			try
			{
				if (shopId == 0)
				{
					return BadRequest(new Status());
				}
				if (string.IsNullOrEmpty(couponCode))
				{
					return BadRequest(new Status());
				}

				List<CouponResponseDTO> coupons = _mapper.Map<List<CouponResponseDTO>>(_couponRepository.GetCoupons(shopId, couponCode));

				return Ok(coupons);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}

		#region Get list coupons

		[Authorize("Seller")]
		[HttpPost("List/Seller")]
		public IActionResult GetListCouponsSeller(SellerCouponRequestDTO request)
		{
			ResponseData response = new ResponseData();
			try
			{
				if (request == null || request.UserId == 0)
				{
					response.Status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
					response.Status.Ok = false;
					response.Status.Message = "Invalid";
					return Ok(response);
				}

				bool isShop = _shopRepository.UserHasShop(request.UserId);
				if (!isShop)
				{
					response.Status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
					response.Status.Ok = false;
					response.Status.Message = "Invalid";
					return Ok(response);
				}

				string formatDate = "M/d/yyyy";
				CultureInfo provider = CultureInfo.InvariantCulture;
				DateTime? startDate = string.IsNullOrEmpty(request.StartDate) ? null : DateTime.ParseExact(request.StartDate.Trim(), formatDate, provider);
				DateTime? endDate = string.IsNullOrEmpty(request.EndDate) ? null : DateTime.ParseExact(request.EndDate.Trim(), formatDate, provider);

				List<Coupon> coupons = _couponRepository.GetListCouponsByShop(request.UserId, request.CouponCode.Trim(), startDate, endDate, request.Status);
				response.Status.ResponseCode = Constants.RESPONSE_CODE_SUCCESS;
				response.Status.Message = "Success";
				response.Status.Ok = true;
				response.Result = coupons.Select(x => new SellerCouponResponseDTO
				{
					CouponId = x.CouponId,
					CouponCode = x.CouponCode,
					StartDate = x.StartDate,
					EndDate = x.EndDate,
					Quantity = x.Quantity,
					PriceDiscount = x.PriceDiscount,
					AmountOrderCondition = x.MinTotalOrderValue,
					IsPublic = x.IsPublic
				}).ToList();
			}
			catch (Exception)
			{
				response.Status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
				response.Status.Ok = false;
				response.Status.Message = "Invalid";
				return Ok(response);
			}
			return Ok(response);
		}
		#endregion

		#region add coupon
		[Authorize("Seller")]
		[HttpPost("Add")]
		public IActionResult AddCoupon(SellerAddCouponRequestDTO request)
		{
			ResponseData response = new ResponseData();
			try
			{
				if (request == null || request.UserId == 0 || request.StartDate >= request.EndDate
					|| string.IsNullOrWhiteSpace(request.CouponCode)
					|| request.PriceDiscount > request.AmountOrderCondition)
				{
					response.Status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
					response.Status.Ok = false;
					response.Status.Message = "Invalid";
					return Ok(response);
				}

				bool isShop = _shopRepository.UserHasShop(request.UserId);
				bool couponCodeExist = _couponRepository.CheckCouponCodeExist(request.CouponCode.Trim());
				if (!isShop || couponCodeExist)
				{
					response.Status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
					response.Status.Ok = false;
					response.Status.Message = "Invalid";
					return Ok(response);
				}

				Coupon coupon = new Coupon
				{
					IsPublic = request.IsPublic,
					IsActive = true,
					StartDate = request.StartDate,
					EndDate = request.EndDate,
					CouponName = $"Discount {request.PriceDiscount}đ for amount order at least {request.AmountOrderCondition}đ.",
					CouponCode = request.CouponCode,
					PriceDiscount = request.PriceDiscount,
					MinTotalOrderValue = request.AmountOrderCondition,
					Quantity = request.Quantity,
					ShopId = request.UserId
				};
				_couponRepository.AddCoupon(coupon);

				response.Status.ResponseCode = Constants.RESPONSE_CODE_SUCCESS;
				response.Status.Message = "Success";
				response.Status.Ok = true;
			}
			catch (Exception)
			{
				response.Status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
				response.Status.Ok = false;
				response.Status.Message = "Invalid";
				return Ok(response);
			}
			return Ok(response);
		}
		#endregion

		#region update status coupon
		[Authorize("Seller")]
		[HttpPost("Edit/Status")]
		public IActionResult UpdateStatusCoupon(SellerUpdateStatusCouponDTO request)
		{
			ResponseData response = new ResponseData();
			try
			{
				if (request == null || request.UserId == 0)
				{
					response.Status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
					response.Status.Ok = false;
					response.Status.Message = "Invalid";
					return Ok(response);
				}

				bool isShop = _shopRepository.UserHasShop(request.UserId);
				if (!isShop)
				{
					response.Status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
					response.Status.Ok = false;
					response.Status.Message = "Invalid";
					return Ok(response);
				}
				Coupon? coupon = _couponRepository.GetCoupons(request.CouponId);
				if (coupon == null)
				{
					response.Status.ResponseCode = Constants.RESPONSE_CODE_DATA_NOT_FOUND;
					response.Status.Ok = false;
					response.Status.Message = "Invalid";
					return Ok(response);
				}
				coupon.IsPublic = request.IsPublic;
				_couponRepository.UpdateCoupon(coupon);

				response.Status.ResponseCode = Constants.RESPONSE_CODE_SUCCESS;
				response.Status.Message = "Success";
				response.Status.Ok = true;
			}
			catch (Exception)
			{
				response.Status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
				response.Status.Ok = false;
				response.Status.Message = "Invalid";
				return Ok(response);
			}
			return Ok(response);
		}
		#endregion


		#region Remove coupon
		[Authorize("Seller")]
		[HttpPost("Remove")]
		public IActionResult UpdateStatusCoupon(SellerRemoveCouponRequestDTO request)
		{
			ResponseData response = new ResponseData();
			try
			{
				if (request == null || request.UserId == 0)
				{
					response.Status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
					response.Status.Ok = false;
					response.Status.Message = "Invalid";
					return Ok(response);
				}

				bool isShop = _shopRepository.UserHasShop(request.UserId);
				if (!isShop)
				{
					response.Status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
					response.Status.Ok = false;
					response.Status.Message = "Invalid";
					return Ok(response);
				}
				Coupon? coupon = _couponRepository.GetCoupons(request.CouponId);
				if (coupon == null)
				{
					response.Status.ResponseCode = Constants.RESPONSE_CODE_DATA_NOT_FOUND;
					response.Status.Ok = false;
					response.Status.Message = "Invalid";
					return Ok(response);
				}
				coupon.IsActive = false;
				_couponRepository.UpdateCoupon(coupon);

				response.Status.ResponseCode = Constants.RESPONSE_CODE_SUCCESS;
				response.Status.Message = "Success";
				response.Status.Ok = true;
			}
			catch (Exception)
			{
				response.Status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
				response.Status.Ok = false;
				response.Status.Message = "Invalid";
				return Ok(response);
			}
			return Ok(response);
		}
		#endregion



		[HttpGet("CheckCouponCodeExist/{couponCode}")]
		public IActionResult CheckCouponCodeExist(string couponCode)
		{
			ResponseData response = new ResponseData();
			if (string.IsNullOrWhiteSpace(couponCode))
			{
				response.Status.Ok = false;
				response.Status.Message = "Invalid";
				response.Status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
				return Ok(response);
			}

			bool result = _couponRepository.CheckCouponCodeExist(couponCode.Trim());
			response.Status.Ok = !result;
			response.Status.Message = result ? "Invalid" : "Success";
			response.Status.ResponseCode = result ? Constants.RESPONSE_CODE_FAILD : Constants.RESPONSE_CODE_SUCCESS;
			return Ok(response);
		}
	}
}
