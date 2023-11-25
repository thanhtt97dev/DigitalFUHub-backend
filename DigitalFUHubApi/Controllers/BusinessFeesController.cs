using AutoMapper;
using Comons;
using DataAccess.IRepositories;
using DigitalFUHubApi.Comons;
using DTOs.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DigitalFUHubApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class BusinessFeesController : ControllerBase
	{
		private readonly IBusinessFeeRepository businessFeeRepository;
		private readonly IMapper mapper;

		public BusinessFeesController(IBusinessFeeRepository businessFeeRepository, IMapper mapper)
		{
			this.businessFeeRepository = businessFeeRepository;
			this.mapper = mapper;
		}

		#region Get all business fee
		[Authorize(Roles ="Admin")]
		[HttpPost("GetBusinessFee")]
		public IActionResult GetBusinessFee(BusinessFeeRequestDTO request)
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

				var fees = businessFeeRepository.GetBusinessFee(businessFeeId, request.MaxFee, fromDate, toDate);
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, fees));
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Add new business fee
		[Authorize(Roles = "Admin")]
		[HttpPost("AddNewBusinessFee")]
		public IActionResult AddNewBusinessFee(CreateBusinessFeeRequestDTO request)
		{
			if (!ModelState.IsValid) return BadRequest();

			ResponseData responseData = new ResponseData();
			try
			{
				businessFeeRepository.AddNewBusinessFee(request.Fee);

				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, new {}));
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion
	}
}
