using AutoMapper;
using Azure.Core;
using BusinessObject.Entities;
using Comons;
using DataAccess.IRepositories;
using DataAccess.Repositories;
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
		public IActionResult GetHistoryTransactionInternal(HistoryTransactionInternalRequestDTO request)
		{
			if (!ModelState.IsValid) return BadRequest();

			const int TRANSACTION_TYPE_ALL = 0;
			int[] transactionTypes = Constants.TRANSACTION_INTERNAL_STATUS_TYPE;
			if (!transactionTypes.Contains(request.TransactionInternalTypeId) && request.TransactionInternalTypeId != TRANSACTION_TYPE_ALL)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid transaction type!", false, new()));
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

				var totalRecord = transactionRepository.GetNumberTransactionInternal(orderId, request.Email, fromDate, toDate, request.TransactionInternalTypeId);
				var numberPages = totalRecord / Constants.PAGE_SIZE + 1;
				if (request.Page > numberPages || request.Page == 0)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid number page", false, new()));
				}

				var transactions = transactionRepository.GetHistoryTransactionInternal(orderId, request.Email, fromDate, toDate, request.TransactionInternalTypeId, request.Page);

				var result = new 
				{
					Total = totalRecord,
					TransactionInternals = mapper.Map<List<HistoryTransactionInternalResponseDTO>>(transactions)
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
