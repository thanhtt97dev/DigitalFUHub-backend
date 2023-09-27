using AutoMapper;
using DataAccess.DAOs;
using DataAccess.IRepositories;
using DataAccess.Repositories;
using DigitalFUHubApi.Comons;
using DigitalFUHubApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BusinessObject.Entities;
using DTOs.Bank;
using DTOs.MbBank;

namespace DigitalFUHubApi.Controllers
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
			ResponseData responseData = new ResponseData();
			Status status = new Status();
			try
			{
				if (inquiryAccountNameRequestDTO == null || 
					string.IsNullOrEmpty(inquiryAccountNameRequestDTO.BankId) || 
					string.IsNullOrEmpty(inquiryAccountNameRequestDTO.CreditAccount))
				{
					return BadRequest();
				}
				MbBankResponse? mbBankResponse = await mbBankService.InquiryAccountName(inquiryAccountNameRequestDTO);
				if (mbBankResponse == null) 
					return StatusCode(StatusCodes.Status500InternalServerError, "Server err!");

				if (mbBankResponse.Code == Constants.MB_BANK_RESPONE_CODE_SAME_URI_IN_SAME_TIME)
				{
					status.Message = "Request many time in same time!";
					status.Ok = false;
					status.ResponseCode = Constants.RESPONSE_CODE_FAILD;
					responseData.Status = status;
				}else if (mbBankResponse.Code == Constants.MB_BANK_RESPONE_CODE_SESSION_INVALID)
				{
					status.Message = "Third-party's session invalid";
					status.Ok = false;
					status.ResponseCode = Constants.RESPONSE_CODE_FAILD;
					responseData.Status = status;
				}
				else if (mbBankResponse.Code == Constants.MB_BANK_RESPONE_CODE_SEARCH_WITH_TYPE_ERR)
				{
					status.Message = "Search data err";
					status.Ok = false;
					status.ResponseCode = Constants.RESPONSE_CODE_FAILD;
					responseData.Status = status;
				}
				else if (mbBankResponse.Code == Constants.MB_BANK_RESPONE_CODE_ACCOUNT_NOT_FOUND)
				{
					status.Message = "Not found";
					status.Ok = false;
					status.ResponseCode = Constants.RESPONSE_CODE_DATA_NOT_FOUND;
					responseData.Status = status;
				}
				else if (mbBankResponse.Code == Constants.MB_BANK_RESPONE_CODE_SUCCESS)
				{
					status.Message = "Success";
					status.Ok = true;
					status.ResponseCode = Constants.RESPONSE_CODE_SUCCESS;
					responseData.Status = status;
					responseData.Result = mbBankResponse.Result;
				}

				return Ok(responseData);
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Add user's bank account
		/// <summary>
		///		Add user's bank account link with DigitalFUHub
		///		//RULE:
		///		1. If user was have > 1 bank account  
		///		2. If Mb bank can not response data
		///		3. If user's bank account not found
		///		=> Not accept
		/// </summary>
		/// <param name="UserId"></param>
		/// <param name="BankId"></param>
		/// <param name="CreditAccount"></param>
		[HttpPost("AddBankAccount")]
		public async Task<IActionResult> AddBankAccount(BankLinkAccountRequestDTO bankLinkAccountRequestDTO)
		{
			ResponseData responseData = new ResponseData();
			Status status = new Status();
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
				if (user == null) return Conflict("User not existed");

				//RULE 1
				var totalUserLinkedBank = bankRepository.TotalUserLinkedBank(bankLinkAccountRequestDTO.UserId);
				if (totalUserLinkedBank > 1)
				{
					status.Message = "You was linked with bank account!";
					status.Ok = false;
					status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
					responseData.Status = status;
					return Ok(responseData);
				}

				BankInquiryAccountNameRequestDTO bankInquiryAccount = new BankInquiryAccountNameRequestDTO()
				{
					BankId = bankLinkAccountRequestDTO.BankId,
					CreditAccount = bankLinkAccountRequestDTO.CreditAccount,
				};

				var mbBank = await mbBankService.InquiryAccountName(bankInquiryAccount);
				
				if (mbBank == null || mbBank.Result == null) //RULE 2
				{
					status.Message = "Third-party's server err";
					status.Ok = false;
					status.ResponseCode = Constants.RESPONSE_CODE_FAILD;
					responseData.Status = status;
					return Ok(responseData);
				}
				else
				{
					if (mbBank.Code != Constants.MB_BANK_RESPONE_CODE_SUCCESS)
					{
						if (mbBank.Code == Constants.MB_BANK_RESPONE_CODE_ACCOUNT_NOT_FOUND) //RULE 3
						{
							status.Message = "Not found";
							status.Ok = false;
							status.ResponseCode = Constants.RESPONSE_CODE_DATA_NOT_FOUND;
							responseData.Status = status;
						}
						else //RULE 2
						{
							status.Message = "Third-party's server err";
							status.Ok = false;
							status.ResponseCode = Constants.RESPONSE_CODE_FAILD;
							responseData.Status = status;
						}
						return Ok(responseData);
					}
				}
				var benName = mbBank.Result;
				UserBank userBank = new UserBank()
				{
					BankId = long.Parse(bankLinkAccountRequestDTO.BankId),
					UserId = bankLinkAccountRequestDTO.UserId,
					CreditAccount = bankLinkAccountRequestDTO.CreditAccount,
					CreditAccountName = benName.ToString() ?? string.Empty,
					UpdateAt = DateTime.UtcNow,	
				};

				bankRepository.AddUserBank(userBank);

				status.Message = "Add bank account success!";
				status.Ok = false;
				status.ResponseCode = Constants.RESPONSE_CODE_SUCCESS;
				responseData.Status = status;
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Update user's bank account
		/// <summary>
		///		Update user's bank account link with DigitalFUHub
		///		//RULE:
		///		1. User can update bank account if updated date is less than 15 days with current day
		///		2. If Mb bank can not response data
		///		3. If user's bank account not found
		///		=> Not accept
		/// </summary>
		/// <param name="UserId"></param>
		/// <param name="BankId"></param>
		/// <param name="CreditAccount"></param>
		[HttpPost("UpdateBankAccount")]
		public async Task<IActionResult> UpdateBankAccount(BankLinkAccountRequestDTO bankLinkAccountRequestDTO)
		{
			ResponseData responseData = new ResponseData();
			Status status = new Status();
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
				if (user == null) return Conflict("User not existed");

				var userBankAccount = bankRepository.GetUserBank(bankLinkAccountRequestDTO.UserId);
				if (userBankAccount == null) return Conflict("User not have bank account to update!");

				
				bool acceptUpdate = Util.CompareDateEqualGreaterThanDaysCondition(userBankAccount.UpdateAt, Constants.NUMBER_DAYS_CAN_UPDATE_BANK_ACCOUNT);
				if (!acceptUpdate) //RULE 1
				{
					status.Message = "After 15 days can update";
					status.Ok = false;
					status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
					responseData.Status = status;
					return Ok(responseData);
				}

				BankInquiryAccountNameRequestDTO bankInquiryAccount = new BankInquiryAccountNameRequestDTO()
				{
					BankId = bankLinkAccountRequestDTO.BankId,
					CreditAccount = bankLinkAccountRequestDTO.CreditAccount,
				};

				var mbBank = await mbBankService.InquiryAccountName(bankInquiryAccount);
				if (mbBank == null || mbBank.Result == null) //RULE2 2
				{
					status.Message = "Third-party's server err";
					status.Ok = false;
					status.ResponseCode = Constants.RESPONSE_CODE_FAILD;
					responseData.Status = status;
					return Ok(responseData);
				}
				else
				{
					if (mbBank.Code != Constants.MB_BANK_RESPONE_CODE_SUCCESS)
					{
						if (mbBank.Code == Constants.MB_BANK_RESPONE_CODE_ACCOUNT_NOT_FOUND) //RULE 3
						{
							status.Message = "Not found";
							status.Ok = false;
							status.ResponseCode = Constants.RESPONSE_CODE_DATA_NOT_FOUND;
							responseData.Status = status;
						}
						else //RULE 2
						{
							status.Message = "Third-party's server err";
							status.Ok = false;
							status.ResponseCode = Constants.RESPONSE_CODE_FAILD;
							responseData.Status = status;
						}
						return Ok(responseData);
					}
				}
				var benName = mbBank.Result;
				UserBank userBank = new UserBank()
				{
					BankId = long.Parse(bankLinkAccountRequestDTO.BankId),
					UserId = bankLinkAccountRequestDTO.UserId,
					CreditAccount = bankLinkAccountRequestDTO.CreditAccount,
					CreditAccountName = benName.ToString() ?? string.Empty,
					UpdateAt = DateTime.UtcNow,	
				};

				bankRepository.UpdateUserBank(userBank);

				status.Message = "Update user's bank account success!";
				status.Ok = true;
				status.ResponseCode = Constants.RESPONSE_CODE_SUCCESS;
				responseData.Status = status;

				return Ok(responseData);
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
				transaction.Code = Util.GetRandomString(10) + depositTransactionDTO.UserId + Constants.BANK_TRANSACTION_CODE_KEY;
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

		#region Get history deposit transaction
		[HttpGet("HistoryDeposit/{id}")]
		public IActionResult GetHistoryDepositTransaction(int userId)
		{
			try
			{
				if (userId == 0) return BadRequest();

				
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
