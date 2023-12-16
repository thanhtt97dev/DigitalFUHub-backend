using AutoMapper;
using Comons;
using DataAccess.IRepositories;
using DataAccess.Repositories;
using DigitalFUHubApi.Comons;
using DTOs.Admin;
using DTOs.ShopRegisterFee;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DigitalFUHubApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ShopRegisterFeesController : ControllerBase
	{
		private readonly IShopRegisterFeeRepository shopRegisterFeeRepository;
		private readonly IMapper mapper;

		public ShopRegisterFeesController(IShopRegisterFeeRepository shopRegisterFeeRepository, IMapper mapper)
		{
			this.shopRegisterFeeRepository = shopRegisterFeeRepository;
			this.mapper = mapper;
		}

		[Authorize(Roles = "Admin")]
		[HttpPost("GetFees")]
		public IActionResult GetFees(BusinessFeeRequestDTO request)
		{
			if (!ModelState.IsValid) return BadRequest();
			try
			{
				(bool isValid, DateTime? fromDate, DateTime? toDate) = Util.GetFromDateToDate(request.FromDate, request.ToDate);
				if (!isValid)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid date", false, new()));
				}

				long businessFeeId;
				long.TryParse(request.BusinessFeeId, out businessFeeId);

				var fees = shopRegisterFeeRepository.GetFees(businessFeeId, request.MaxFee, fromDate, toDate);

				var result = mapper.Map<List<ShopRegisterFeeResponseDTO>>(fees);

				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, result));
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}

		[Authorize(Roles = "Admin")]
		[HttpPost("AddNew")]
		public IActionResult AddNewBusinessFee(CreateBusinessFeeRequestDTO request)
		{
			if (!ModelState.IsValid) return BadRequest();
			try
			{
				if (request.Fee < Constants.MIN_SELLER_REGISTRATION_FEE)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid Fee", true, new { }));
				}

				shopRegisterFeeRepository.AddNewShopRegisterFee(request.Fee);

				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, new { }));
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#region Add new business fee

		#endregion
	}
}
