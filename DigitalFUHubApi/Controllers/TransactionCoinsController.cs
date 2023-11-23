using AutoMapper;
using Azure.Core;
using Comons;
using DataAccess.IRepositories;
using DigitalFUHubApi.Comons;
using DTOs.Bank;
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
		public IActionResult GetHistoryTransactionInternal(HistoryTransactionCoinRequestDTO request)
		{
			if (!ModelState.IsValid) return BadRequest();

			const int TRANSACTION_TYPE_ALL = 0;
			int[] transactionTypes = Constants.TRANSACTION_COIN_STATUS_TYPE;
			if (!transactionTypes.Contains(request.TransactionCoinTypeId) && request.TransactionCoinTypeId != TRANSACTION_TYPE_ALL)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid transaction type", false, new()));
			}

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
				
				long orderId;
				long.TryParse(request.OrderId, out orderId);

				var totalRecord = transactionCoinRepository.GetNumberTransactionCoin(orderId, request.Email, fromDate, toDate, request.TransactionCoinTypeId);
				var numberPages = totalRecord / Constants.PAGE_SIZE + 1;
				if (request.Page > numberPages || request.Page == 0)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid number page", false, new()));
				}

				var transactions = transactionCoinRepository.GetHistoryTransactionCoin(orderId, request.Email, fromDate, toDate, request.TransactionCoinTypeId, request.Page);
				
				var result = new
				{
					Total = totalRecord,
					TransactionCoins = mapper.Map<List<HistoryTransactionCoinResponseDTO>>(transactions)
				};
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, result));
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion
	}
}
