using AutoMapper;
using BusinessObject;
using DataAccess.DAOs;
using DataAccess.IRepositories;
using DataAccess.Repositories;
using DTOs;
using FuMarketAPI.Services;
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

		#region Link bank account with user
		[HttpPost("linkBankAccount")]
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


	}
}
