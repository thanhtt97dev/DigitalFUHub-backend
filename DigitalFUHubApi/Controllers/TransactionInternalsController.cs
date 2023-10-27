using AutoMapper;
using Comons;
using DataAccess.IRepositories;
using DigitalFUHubApi.Comons;
using DTOs.Bank;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DigitalFUHubApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class TransactionInternalsController : ControllerBase
	{
		private readonly ITransactionInternalRepository transactionRepository;
		private readonly IMapper mapper;

		public TransactionInternalsController(ITransactionInternalRepository transactionRepository, IMapper mapper)
		{
			this.transactionRepository = transactionRepository;
			this.mapper = mapper;
		}

		#region Get History transaction of internal 
		[Authorize(Roles = "Admin")]
		[HttpPost("All")]
		public IActionResult GetHistoryTransactionInternal(HistoryTransactionInternalRequestDTO requestDTO)
		{
			if (!ModelState.IsValid) return BadRequest();

			ResponseData responseData = new ResponseData();
			Status status = new Status();

			const int TRANSACTION_TYPE_ALL = 0;
			int[] transactionTypes = Constants.TRANSACTION_STATUS;
			if (!transactionTypes.Contains(requestDTO.TransactionInternalTypeId) && requestDTO.TransactionInternalTypeId != TRANSACTION_TYPE_ALL)
			{
				status.Message = "Invalid transaction type id!";
				status.Ok = false;
				status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
				responseData.Status = status;
				return Ok(responseData);
			}

			try
			{

				DateTime fromDate;
				DateTime toDate;
				string format = "M/d/yyyy";
				try
				{
					fromDate = DateTime.ParseExact(requestDTO.FromDate, format, System.Globalization.CultureInfo.InvariantCulture);
					toDate = DateTime.ParseExact(requestDTO.ToDate, format, System.Globalization.CultureInfo.InvariantCulture).AddDays(1);
					if (fromDate > toDate)
					{
						status.Message = "From date must be less than to date";
						status.Ok = false;
						status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
						responseData.Status = status;
						return Ok(responseData);
					}
				}
				catch (FormatException)
				{
					status.Message = "Invalid datetime";
					status.Ok = false;
					status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
					responseData.Status = status;
					return Ok(responseData);
				}

				long orderId;
				long.TryParse(requestDTO.OrderId, out orderId);

				var transactions = transactionRepository.GetHistoryTransactionInternal(orderId, requestDTO.Email, fromDate, toDate, requestDTO.TransactionInternalTypeId);

				var result = mapper.Map<List<HistoryTransactionInternalResponseDTO>>(transactions);

				status.Message = "Success!";
				status.Ok = true;
				status.ResponseCode = Constants.RESPONSE_CODE_SUCCESS;
				responseData.Status = status;
				responseData.Result = result;
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
