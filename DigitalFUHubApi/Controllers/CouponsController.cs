using AutoMapper;
using Azure;
using BusinessObject.Entities;
using Comons;
using DataAccess.IRepositories;
using DataAccess.Repositories;
using DigitalFUHubApi.Comons;
using DigitalFUHubApi.Services;
using DTOs.Coupon;
using DTOs.Seller;
using Google.Apis.Util;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Text.RegularExpressions;

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

		public CouponsController(IConfiguration configuration,
			IProductRepository productRepository,
			StorageService storageService,
			IShopRepository shopRepository,
			IUserRepository userRepository,
			IRoleRepository roleRepository,
			IOrderRepository orderRepository,
			ICouponRepository couponRepository,
			JwtTokenService jwtTokenService,
			IMapper mapper)
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
				return Ok(response);

			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Get coupon private
		[HttpGet("GetCouponPrivate")]
		public IActionResult GetCouponPrivate(string couponCode, long shopId)
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

				if (shopId == 0)
				{
					response.Status.Ok = false;
					response.Status.Message = "Invalid";
					response.Status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
					return Ok(response);
				}

				CouponResponseDTO coupon = _mapper.Map<CouponResponseDTO>(_couponRepository.GetCouponPrivate(couponCode, shopId));
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
			try
			{
				if (userId != _jwtTokenService.GetUserIdByAccessToken(User))
				{
					return Unauthorized();
				}
				Coupon? coupon = _couponRepository.GetCoupon(couponId, userId);
				if (coupon == null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Not found", false, new()));
				}
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, _mapper.Map<SellerCouponResponseDTO>(coupon)));
			}
			catch (Exception e)
			{

				return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
			}

		}
		#endregion

		#region Get list coupons

		[Authorize("Seller")]
		[HttpPost("Seller/List")]
		public IActionResult GetListCouponsSeller(SellerCouponRequestDTO request)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid data", false, new()));
				}
				if (request.Page <= 0)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid page", false, new()));
				}

				string formatDate = "M/d/yyyy";
				CultureInfo provider = CultureInfo.InvariantCulture;
				DateTime? startDate = string.IsNullOrEmpty(request.StartDate) ? null : DateTime.ParseExact(request.StartDate.Trim(),
					formatDate, provider);
				DateTime? endDate = string.IsNullOrEmpty(request.EndDate) ? null : DateTime.ParseExact(request.EndDate.Trim(),
					formatDate, provider);
				(long totalItems, List<Coupon> coupons) = _couponRepository
					.GetListCouponsByShop(_jwtTokenService.GetUserIdByAccessToken(User),
					request.CouponCode.Trim(), startDate, endDate, request.IsPublic, request.Status, request.Page);
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, new ListCouponResponseDTO
				{
					TotalItems = totalItems,
					Coupons = _mapper.Map<List<SellerCouponResponseDTO>>(coupons)
				}
				));
			}
			catch (Exception e)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
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
				//if (request.UserId != _jwtTokenService.GetUserIdByAccessToken(User))
				//{
				//	return Unauthorized();
				//}
				Regex rxCouponCode = new Regex(Constants.REGEX_COUPON_CODE);
				if (!ModelState.IsValid
					|| string.IsNullOrWhiteSpace(request.CouponCode)
					|| string.IsNullOrWhiteSpace(request.CouponName)
					|| !rxCouponCode.IsMatch(request.CouponCode)
					|| request.MinTotalOrderValue < Constants.MIN_PRICE_OF_MIN_ORDER_TOTAL_VALUE
					|| request.MinTotalOrderValue > Constants.MAX_PRICE_OF_MIN_ORDER_TOTAL_VALUE
					|| request.PriceDiscount < Constants.MIN_PRICE_DISCOUNT_COUPON
					|| (request.MinTotalOrderValue != 0 && request.PriceDiscount > (request.MinTotalOrderValue * Constants.MAX_PERCENTAGE_PRICE_DISCOUNT_COUPON))
					|| (request.MinTotalOrderValue == 0 && request.PriceDiscount > Constants.MAX_PRICE_DISCOUNT_COUPON))
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid data", false, new()));
				}

				DateTime startDT = DateTime.Parse(request.StartDate);
				DateTime endDT = DateTime.Parse(request.EndDate);
				if (endDT.Subtract(startDT) < new TimeSpan(1, 0, 0))
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "The coupon program lasts at least 1 hour",
						false, new()));
				}
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
					ShopId = request.UserId,
					CouponTypeId = request.TypeId
				};
				if (request.TypeId == Constants.COUPON_TYPE_SPECIFIC_PRODUCTS)
				{
					if (request.ProductsApplied == null || request.ProductsApplied.Count == 0)
					{
						return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid products applied", false, new()));
					}
					else
					{
						coupon.CouponProducts = request.ProductsApplied.Select(x => new CouponProduct
						{
							ProductId = x,
							UpdateDate = DateTime.Now,
							IsActivate = true
						}).ToList();
					}

				}
				_couponRepository.AddCoupon(coupon);

				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, new()));
			}
			catch (Exception e)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
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
				//if (request.UserId != _jwtTokenService.GetUserIdByAccessToken(User))
				//{
				//	return Unauthorized();
				//}
				Regex rxCouponCode = new Regex(Constants.REGEX_COUPON_CODE);
				if (!ModelState.IsValid
					|| string.IsNullOrWhiteSpace(request.CouponCode)
					|| string.IsNullOrWhiteSpace(request.CouponName)
					|| !rxCouponCode.IsMatch(request.CouponCode)
					|| request.MinTotalOrderValue < Constants.MIN_PRICE_OF_MIN_ORDER_TOTAL_VALUE
					|| request.MinTotalOrderValue > Constants.MAX_PRICE_OF_MIN_ORDER_TOTAL_VALUE
					|| request.PriceDiscount < Constants.MIN_PRICE_DISCOUNT_COUPON
					|| (request.MinTotalOrderValue != 0 && request.PriceDiscount > (request.MinTotalOrderValue * Constants.MAX_PERCENTAGE_PRICE_DISCOUNT_COUPON))
					)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid data", false, new()));
				}

				DateTime startDT = DateTime.Parse(request.StartDate);
				DateTime endDT = DateTime.Parse(request.EndDate);
				if (endDT.Subtract(startDT) < new TimeSpan(1, 0, 0))
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "The coupon program lasts at least 1 hour",
						false, new()));
				}
				Coupon coupon = new Coupon
				{
					CouponId = request.CouponId,
					IsPublic = request.IsPublic,
					StartDate = startDT,
					EndDate = endDT,
					CouponName = request.CouponName.Trim(),
					CouponCode = request.CouponCode.ToUpper().Trim(),
					PriceDiscount = request.PriceDiscount,
					MinTotalOrderValue = request.MinTotalOrderValue,
					Quantity = request.Quantity,
					ShopId = request.UserId,
					CouponTypeId = request.TypeId,
					CouponProducts = request.ProductsApplied.Select(x => new CouponProduct
					{
						CouponId = request.CouponId,
						ProductId = x,
						IsActivate = true,
						UpdateDate = DateTime.Now,
					}).ToList()
				};
				_couponRepository.UpdateCoupon(coupon);

				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, new()));
			}
			catch (Exception e)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
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
				if (request.UserId != _jwtTokenService.GetUserIdByAccessToken(User))
				{
					return Unauthorized();
				}
				if (!ModelState.IsValid)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid data", false, new()));
				}


				Coupon? coupon = _couponRepository.GetCoupon(request.CouponId, request.UserId);
				if (coupon == null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Not found", false, new()));
				}
				coupon.IsPublic = request.IsPublic;
				_couponRepository.UpdateStatusCoupon(coupon);

				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, new()));
			}
			catch (Exception e)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
			}
		}
		#endregion

		#region update coupon program end
		[Authorize("Seller")]
		[HttpPost("Edit/Finish/{couponId}")]
		public IActionResult UpdateCouponFinish(long couponId)
		{
			try
			{
				_couponRepository.UpdateCouponFinish(couponId, _jwtTokenService.GetUserIdByAccessToken(User));
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, new()));
			}
			catch (Exception e)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_FAILD, e.Message, false, new()));
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
				if (request.UserId != _jwtTokenService.GetUserIdByAccessToken(User))
				{
					return Unauthorized();
				}
				if (!ModelState.IsValid)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid data", false, new()));
				}

				Coupon? coupon = _couponRepository.GetCoupon(request.CouponId, request.UserId);
				if (coupon == null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Not found", false, new()));
				}
				coupon.IsActive = false;
				_couponRepository.UpdateStatusCoupon(coupon);

				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, new()));
			}
			catch (Exception e)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
			}
		}
		#endregion

		#region check coupon exist
		[Authorize("Seller")]
		[HttpGet("IsExistCouponCode/{actionMode}/{couponCode}")]
		public IActionResult CheckCouponCodeExist(string actionMode, string couponCode)
		{
			if (string.IsNullOrWhiteSpace(couponCode) || !Regex.IsMatch(couponCode, "^[a-zA-Z0-9]{4,}$"))
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid", false, new()));
			}
			try
			{
				bool result = _couponRepository.IsExistCouponCode(_jwtTokenService.GetUserIdByAccessToken(User), couponCode.Trim(), actionMode);
				return Ok(new ResponseData(result ? Constants.RESPONSE_CODE_FAILD : Constants.RESPONSE_CODE_SUCCESS,
					result ? "Invalid" : "Success",
					!result,
					new()));
			}
			catch (Exception e)
			{

				return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
			}
		}
		#endregion
	}
}
