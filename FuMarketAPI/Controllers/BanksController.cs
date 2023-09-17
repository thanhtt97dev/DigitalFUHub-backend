using AutoMapper;
using BusinessObject;
using DataAccess.DAOs;
using DataAccess.IRepositories;
using DataAccess.Repositories;
using DTOs;
using FuMarketAPI.Comons;
using FuMarketAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FuMarketAPI.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class BanksController : ControllerBase
	{
		private readonly IMapper mapper;
		private readonly IBankRepository bankRepository;
		private readonly IUserRepository userRepository;
		private readonly MbBankService mbBankService;

		public BanksController(IMapper mapper, IBankRepository bankRepository, IUserRepository userRepository,
			MbBankService mbBankService)
		{
			this.mapper = mapper;
			this.bankRepository = bankRepository;
			this.userRepository = userRepository;
			this.mbBankService = mbBankService;
		}

		

		#region Get all bank info
		[HttpGet("getAll")]
		public IActionResult GetAll()
		{
			try
			{
				var banks = bankRepository.GetAll();
				var result = mapper.Map<List<BankResponeDTO>>(banks);
				return Ok(result);
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Check connect with MB Bank with sessionId
		[HttpGet("connect")]
		public async Task<IActionResult> Connect()
		{
			try
			{
				var canConnect = await mbBankService.TestConnection();
				if (!canConnect) return Conflict("Cannot connect!");
				return Ok();
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Get user bank account
		[HttpGet("user/{id}")]
		public IActionResult CheckUserBankAccount(int id)
		{
			try
			{
				if (id == 0) return BadRequest();
				
				var user = userRepository.GetUserById(id);
				if (user == null) return BadRequest();

				var bank = bankRepository.GetUserBank(id);
				if (bank == null) return Ok(null);
				var result = mapper.Map<BankAccountResponeDTO>(bank);
				return Ok(result);
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Inquiry bank account name
		[HttpPost("InquiryAccountName")]
		public async Task<IActionResult> InquiryAccountName(BankInquiryAccountNameRequestDTO inquiryAccountNameRequestDTO)
		{
			try
			{
				var benName = await mbBankService.InquiryAccountName(inquiryAccountNameRequestDTO);
				if (benName == null) return Conflict("Bank account not existed!");
				return Ok(benName);
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Add bank account with user
		[HttpPost("AddBankAccount")]
		public async Task<IActionResult> LinkBankAccount(BankLinkAccountRequestDTO bankLinkAccountRequestDTO)
		{
			try
			{
				if (bankLinkAccountRequestDTO == null) return BadRequest();
				if (bankLinkAccountRequestDTO.UserId == 0 ||
					string.IsNullOrEmpty(bankLinkAccountRequestDTO.BankId) ||
					string.IsNullOrEmpty(bankLinkAccountRequestDTO.CreditAccount)
				)
				{
					return BadRequest();
				}

				var user = userRepository.GetUserById(bankLinkAccountRequestDTO.UserId);
				if (user == null) return NotFound("User not existed");

				//check rule : 1 user just linked with 1 bank accout
				var totalUserLinkedBank = bankRepository.TotalUserLinkedBank(bankLinkAccountRequestDTO.UserId);
				if (totalUserLinkedBank > 1) return Conflict("You was linked with bank account!");

				BankInquiryAccountNameRequestDTO bankInquiryAccount = new BankInquiryAccountNameRequestDTO()
				{
					BankId = bankLinkAccountRequestDTO.BankId,
					CreditAccount = bankLinkAccountRequestDTO.CreditAccount,
				};

				var benName = await mbBankService.InquiryAccountName(bankInquiryAccount);
				if (benName == null) return Conflict("Bank account not existed!");

				UserBank userBank = new UserBank()
				{
					BankId = long.Parse(bankLinkAccountRequestDTO.BankId),
					UserId = bankLinkAccountRequestDTO.UserId,
					CreditAccount = bankLinkAccountRequestDTO.CreditAccount,
					CreditAccountName = benName.ToString(),
				};

				bankRepository.AddUserBank(userBank);

				return Ok();
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Create Deposit Transaction
		[Authorize]
		[HttpPost("CreateDepositTransaction")]
		public IActionResult CreateDepositTransaction(DepositTransactionRequestDTO depositTransactionDTO)
		{
			try
			{
				var transaction = mapper.Map<DepositTransaction>(depositTransactionDTO);
				transaction.Code = Util.GetRandomString(10) + depositTransactionDTO.UserId + "FU";
				bankRepository.CreateDepositTransaction(transaction);
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
