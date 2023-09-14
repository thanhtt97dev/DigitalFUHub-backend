using AutoMapper;
using BusinessObject;
using DataAccess.IRepositories;
using DTOs;
using FuMarketAPI.Comons;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FuMarketAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class FinancialTransactionController : ControllerBase
	{
		private readonly IFinancialTransactionRepository financialTransactionRepository;
		private readonly IMapper mapper;

		public FinancialTransactionController(IFinancialTransactionRepository _financialTransactionRepository, IMapper _mapper)
		{
			mapper = _mapper;
			financialTransactionRepository = _financialTransactionRepository;	
		}

		#region Create Deposit Transaction
		[Authorize]
		[HttpPost("CreateDepositTransaction")]
		public IActionResult CreateDepositTransaction(DepositTransactionRequestDTO depositTransactionDTO)
		{
			try
			{
				var transaction = mapper.Map<DepositTransaction>(depositTransactionDTO);
				transaction.Code = Util.GetRandomString(10) + depositTransactionDTO.UserId + "FU";
				financialTransactionRepository.CreateDepositTransaction(transaction);
				var result = new DepositTransactionResponeDTO()
				{
					Amount = transaction.Amount,
					Code = transaction.Code
				};
				return Ok(result);
			}
			catch (Exception)
			{
				return Conflict("Some things went wrong!");
			}

		}
		#endregion

	}
}
