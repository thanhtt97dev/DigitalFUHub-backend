using AutoMapper;
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
using System.Reflection.Metadata;

namespace DigitalFUHubApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CouponsController : ControllerBase
	{
		
		private readonly ICouponRepository _couponRepository;
		private readonly IMapper _mapper;

		public CouponsController(ICouponRepository couponRepository, IMapper mapper)
		{
			_couponRepository = couponRepository;
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
	}
}
