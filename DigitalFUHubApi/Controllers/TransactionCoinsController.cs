using AutoMapper;
using Comons;
using DataAccess.IRepositories;
using DigitalFUHubApi.Comons;
using DTOs.TransactionCoin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DigitalFUHubApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class TransactionCoinsController : ControllerBase
	{
		private readonly ITransactionCoinRepository transactionCoinRepository;
		private readonly IMapper mapper;

		public TransactionCoinsController(ITransactionCoinRepository transactionCoinRepository, IMapper mapper)
		{
			this.transactionCoinRepository = transactionCoinRepository;
			this.mapper = mapper;
		}

		#region Get History transaction coin 
		[Authorize(Roles = "Admin")]
		[HttpPost("GetHistoryTransactionCoin")]
		public IActionResult GetHistoryTransactionInternal(HistoryTransactionCoinRequestDTO requestDTO)
		{
			if (!ModelState.IsValid) return BadRequest();

			ResponseData responseData = new ResponseData();
			Status status = new Status();

			const int TRANSACTION_TYPE_ALL = 0;
			int[] transactionTypes = Constants.TRANSACTION_COIN_STATUS_TYPE;
			if (!transactionTypes.Contains(requestDTO.TransactionCoinTypeId) && requestDTO.TransactionCoinTypeId != TRANSACTION_TYPE_ALL)
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

				var transactions = transactionCoinRepository.GetHistoryTransactionInternal(orderId, requestDTO.Email, fromDate, toDate, requestDTO.TransactionCoinTypeId);

				var result = mapper.Map<List<HistoryTransactionCoinResponseDTO>>(transactions);


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
