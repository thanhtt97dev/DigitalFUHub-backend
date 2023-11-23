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

			ResponseData responseData = new ResponseData();
			try
			{
				DateTime? fromDate = null;
				DateTime? toDate = null;
				string format = "M/d/yyyy";
				if (!string.IsNullOrEmpty(request.FromDate) && !string.IsNullOrEmpty(request.ToDate))
				{
					try
					{
						fromDate = DateTime.ParseExact(request.FromDate, format, System.Globalization.CultureInfo.InvariantCulture);
						toDate = DateTime.ParseExact(request.ToDate, format, System.Globalization.CultureInfo.InvariantCulture).AddDays(1);
						if (fromDate > toDate)
						{
							return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "From date must be less than to date", false, new()));
						}
					}
					catch (FormatException)
					{
						return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid datetime", false, new()));
					}
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

				responseData.Status.ResponseCode = Constants.RESPONSE_CODE_SUCCESS;
				responseData.Status.Ok = true;
				responseData.Status.Message = "Success!";
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion
	}
}
