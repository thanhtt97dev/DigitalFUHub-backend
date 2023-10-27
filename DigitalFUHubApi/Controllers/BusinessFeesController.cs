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
				DateTime fromDate;
				DateTime toDate;
				string format = "M/d/yyyy";
				try
				{
					fromDate = DateTime.ParseExact(request.FromDate, format, System.Globalization.CultureInfo.InvariantCulture);
					toDate = DateTime.ParseExact(request.ToDate, format, System.Globalization.CultureInfo.InvariantCulture).AddDays(1);
					if (fromDate > toDate)
					{
						responseData.Status.Message = "From date must be less than to date";
						responseData.Status.Ok = false;
						responseData.Status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
						return Ok(responseData);
					}
				}
				catch (FormatException)
				{
					responseData.Status.Message = "Invalid datetime";
					responseData.Status.Ok = false;
					responseData.Status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
					return Ok(responseData);
				}

				long businessFeeId;
				long.TryParse(request.BusinessFeeId, out businessFeeId);

				var fees = businessFeeRepository.GetBusinessFee(businessFeeId, request.MaxFee, fromDate, toDate);
				responseData.Status.ResponseCode = Constants.RESPONSE_CODE_SUCCESS;
				responseData.Status.Ok = true;
				responseData.Status.Message = "Success!";
				responseData.Result = fees;
				return Ok(responseData);
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
