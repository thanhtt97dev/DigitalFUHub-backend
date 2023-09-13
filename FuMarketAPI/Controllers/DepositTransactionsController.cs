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
	public class DepositTransactionsController : ControllerBase
	{
		private readonly IDepositTransactionRepository depositTransactionRepository;
		private readonly IMapper mapper;

		public DepositTransactionsController(IDepositTransactionRepository _depositTransactionRepository, IMapper _mapper)
		{
			mapper = _mapper;
			depositTransactionRepository = _depositTransactionRepository;	
		}

		[Authorize]
		[HttpPost("CreateRequestDeposit")]
		public IActionResult Post(DepositTransactionRequestDTO depositTransactionDTO)

		{
			try
			{
				var transaction = mapper.Map<DepositTransaction>(depositTransactionDTO);
				transaction.Code = Util.GetRandomString(10);
				depositTransactionRepository.CreateTransaction(transaction);
				return Ok(transaction.Code);
			}
			catch (Exception)
			{
				return Conflict("Some things went wrong!");
			}

		}
	}
}
