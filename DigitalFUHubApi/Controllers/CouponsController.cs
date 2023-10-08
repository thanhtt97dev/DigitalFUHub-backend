using AutoMapper;
using Comons;
using DataAccess.IRepositories;
using DataAccess.Repositories;
using DigitalFUHubApi.Comons;
using DTOs.Coupon;
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

        [HttpGet("getByShopId/{shopId}")]

        public IActionResult GetCoupons(long shopId = 0) {
            try
            {
                if (shopId == 0)
                {
                    return BadRequest(new Status());
                }
                List<CouponResponseDTO> coupons = _mapper.Map<List<CouponResponseDTO>>(_couponRepository.GetByShopId(shopId));

                return Ok(coupons) ;
            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
