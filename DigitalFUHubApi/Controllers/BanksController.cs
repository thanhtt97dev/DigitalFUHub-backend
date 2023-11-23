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
using System.Security.Cryptography.Xml;
using Microsoft.Data.SqlClient.Server;
using Comons;
using System.Transactions;
using DTOs.Admin;
using Azure.Core;

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
		[Authorize]		
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
		[Authorize]		
		[HttpGet("user/{id}")]
		public IActionResult GetUserBankAccount(int id)
		{
			ResponseData responseData = new ResponseData();
			Status status = new Status();
			try
			{
				if (id == 0) return BadRequest();

				var user = userRepository.GetUserById(id);
				if (user == null) return BadRequest();

				var bank = bankRepository.GetUserBank(id);
				if (bank == null)
				{
					status.Message = "Not found user's bank account!";
					status.Ok = false;
					status.ResponseCode = Constants.RESPONSE_CODE_DATA_NOT_FOUND;
					responseData.Status = status;
					return Ok(responseData);
				}

				var result = mapper.Map<BankAccountResponeDTO>(bank);
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

		#region Inquiry bank account name
		[Authorize]
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
				}
				else if (mbBankResponse.Code == Constants.MB_BANK_RESPONE_CODE_SESSION_INVALID)
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
		[Authorize]
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
					UpdateAt = DateTime.Now,
					isActivate = true
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
		[Authorize]
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
					UpdateAt = DateTime.Now,
					isActivate = true,
				};

				bankRepository.UpdateUserBankStatus(userBankAccount);
				bankRepository.AddUserBank(userBank);

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
		public IActionResult CreateDepositTransaction(CreateTransactionRequestDTO request)
		{
			if (!ModelState.IsValid) return BadRequest();
			try
			{
				if (!(request.Amount >= Constants.MIN_PRICE_CAN_DEPOSIT && request.Amount <= Constants.MAX_PRICE_CAN_DEPOSIT))
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid feedback type", false, new()));
				}

				var numberDepositTransactionMakedInToday = bankRepository.GetNumberDepositTransactionMakedInToday(request.UserId);
				if (numberDepositTransactionMakedInToday > Constants.NUMBER_DEPOSIT_REQUEST_CAN_MAKE_A_DAY)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_BANK_CUSTOMER_REQUEST_DEPOSIT_EXCEEDED_REQUESTS_CREATED, "Exceeded number of requests created!", false, new()));
				}

				var transaction = mapper.Map<DepositTransaction>(request);
				transaction.Code = Util.GetRandomString(10) + request.UserId + Constants.BANK_TRANSACTION_CODE_KEY;
				bankRepository.CreateDepositTransaction(transaction);
				var result = new DepositTransactionResponeDTO()
				{
					Amount = transaction.Amount,
					Code = transaction.Code
				};

				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success!", true, result));
			}
			catch (Exception)
			{
				return Conflict("Some things went wrong!");
			}

		}
		#endregion

		#region Create Withdraw Transaction
		[Authorize]
		[HttpPost("CreateWithdrawTransaction")]
		public IActionResult CreateWithdrawTransaction(CreateTransactionRequestDTO request)
		{
			if (!ModelState.IsValid) return BadRequest();
			try
			{
				if (request.UserId == 0 || request.Amount == 0)
				{
					return BadRequest();
				}
				
				if(!(request.Amount >= Constants.MIN_PRICE_CAN_WITHDRAW && request.Amount <= Constants.MAX_PRICE_CAN_WITHDRAW))
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid feedback type", false, new()));
				}

				var customer = userRepository.GetUserById(request.UserId);
				if(customer == null) 
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "User not found!", false, new()));
				}

				if(customer.Status == false)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "User has been baned!", false, new()));
				}

				if (customer.AccountBalance < request.Amount)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_BANK_CUSTOMER_REQUEST_WITHDRAW_INSUFFICIENT_BALANCE, "Insufficient balance!", false, new()));
				}

				(int numberWithdrawRequestMakedToday, long totalAnountWithdrawRequestMakedToday) = bankRepository.GetDataWithdrawTransactionMakedToday(request.UserId);

				if (numberWithdrawRequestMakedToday > Constants.NUMBER_WITH_DRAW_REQUEST_CAN_MAKE_A_DAY) 
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_BANK_CUSTOMER_REQUEST_WITHDRAW_EXCEEDED_REQUESTS_CREATED, "Exceeded number of requests created!", false, new()));
				}

				if(totalAnountWithdrawRequestMakedToday > Constants.MAX_PRICE_CAN_WITHDRAW)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_BANK_CUSTOMER_REQUEST_WITHDRAW_EXCEEDED_AMOUNT_A_DAY, "Exceeded total amount can make a day", false, totalAnountWithdrawRequestMakedToday));
				}

				// create withdraw tranascation
				var transaction = mapper.Map<WithdrawTransaction>(request);
				transaction.Code = Util.GetRandomString(10) + request.UserId + Constants.BANK_TRANSACTION_CODE_KEY;
				bankRepository.CreateWithdrawTransaction(transaction);

				var accountBalance = customer.AccountBalance - request.Amount;

				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success!", true, accountBalance));
			}
			catch (Exception)
			{
				return Conflict("Some things went wrong!");
			}

		}
		#endregion

		#region Get history deposit transaction of a user
		[Authorize]
		[HttpPost("HistoryDeposit/{id}")]
		public IActionResult GetHistoryDepositTransaction(int id, HistoryDepositRequestDTO request)
		{
			ResponseData responseData = new ResponseData();
			Status status = new Status();
			string format = "M/d/yyyy";
			try
			{
				if (id == 0 || request == null ||
					request.FromDate == null ||
					request.ToDate == null) return BadRequest();

				DateTime? fromDate = null;
				DateTime? toDate = null;
				if (!string.IsNullOrEmpty(request.FromDate) && !string.IsNullOrEmpty(request.FromDate))
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

				long depositTransactionId;
				long.TryParse(request.DepositTransactionId, out depositTransactionId);

				// 0 : All, 1: paid, 2: unpay
				if(request.Status != 0 && request.Status != 1 && request.Status != 2)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid transaction's status", false, new()));
				}

				var totalRecord = bankRepository.GetNumberDepositTransaction(id, depositTransactionId, string.Empty, fromDate, toDate, request.Status);
				var numberPages = totalRecord / Constants.PAGE_SIZE + 1;
				if (request.Page > numberPages || request.Page == 0)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid number page", false, new()));
				}

				var deposits = bankRepository.GetDepositTransaction(id, depositTransactionId, fromDate, toDate, request.Status, request.Page);
				var result = new HistoryDepositResponseDTO
				{
					Total = totalRecord,
					DepositTransactions = mapper.Map<List<HistoryDepositResponeDTO>>(deposits)
				};

				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, result));
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Get history deposit transaction sucess for admin
		[Authorize(Roles = "Admin")]
		[HttpPost("HistoryDeposit")]
		public IActionResult GetHistoryDepositTransactionSuccess(HistoryDepositForAdminRequestDTO request)
		{
			ResponseData responseData = new ResponseData();
			Status status = new Status();
			string format = "M/d/yyyy";
			try
			{
				if (request == null || request.Email == null ||
					request.FromDate == null || request.ToDate == null) return BadRequest();

				DateTime? fromDate = null;
				DateTime? toDate = null;
				if (!string.IsNullOrEmpty(request.FromDate) && !string.IsNullOrEmpty(request.FromDate))
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
				

				long depositTransactionId;
				long.TryParse(request.DepositTransactionId, out depositTransactionId);

				var totalRecord = bankRepository.GetNumberDepositTransaction(0, depositTransactionId, request.Email, fromDate, toDate, 1);
				var numberPages = totalRecord / Constants.PAGE_SIZE + 1;
				if (request.Page > numberPages || request.Page == 0)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid number page", false, new()));
				}

				var deposits = bankRepository.GetDepositTransactionSucess(depositTransactionId, request.Email, fromDate, toDate, request.Page);
				var result = new HistoryDepositResponseDTO
				{
					Total = totalRecord,
					DepositTransactions = mapper.Map<List<HistoryDepositResponeDTO>>(deposits)
				};

				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, result));
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Get history withdraw transaction of a user
		[Authorize]
		[HttpPost("HistoryWithdraw/{id}")]
		public IActionResult GetHistoryWithdrawTransaction(int id, HistoryWithdrawRequestDTO request)
		{
			ResponseData responseData = new ResponseData();
			Status status = new Status();
			string format = "M/d/yyyy";
			try
			{
				if (id == 0 || request == null || request.FromDate == null ||
					request.ToDate == null) return BadRequest();

				if (!Constants.WITHDRAW_TRANSACTION_STATUS.Contains(request.Status) &&
					request.Status != Constants.WITHDRAW_TRANSACTION_ALL)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid transaction's status", false, new()));
				}

				DateTime? fromDate = null;
				DateTime? toDate = null;

				if(!string.IsNullOrEmpty(request.FromDate) && !string.IsNullOrEmpty(request.FromDate))
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


				long withdrawTransactionId;
				long.TryParse(request.WithdrawTransactionId, out withdrawTransactionId);

				var totalRecord = bankRepository.GetNumberWithdrawTransactionWithCondition(id, withdrawTransactionId, fromDate, toDate, request.Status);
				var numberPages = totalRecord / Constants.PAGE_SIZE + 1;
				if (request.Page > numberPages || request.Page == 0)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid number page", false, new()));
				}

				var withdraws = bankRepository.GetWithdrawTransaction(id, withdrawTransactionId, fromDate, toDate, request.Status, request.Page);
				var result = new HistoryWithdrawResponseDTO
				{
					Total = totalRecord,
					WithdrawTransactions = mapper.Map<List<HistoryWithdrawDetail>>(withdraws)
				};

				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", false, result));
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Get history withdraw transaction for admin
		[Authorize(Roles ="Admin")]
		[HttpPost("HistoryWithdrawAll")]
		public IActionResult GetHistoryWithdrawTransactionForAdmin(HistoryWithdrawRequestDTO requestDTO)
		{
			ResponseData responseData = new ResponseData();
			Status status = new Status();
			string format = "M/d/yyyy";
			try
			{
				if (requestDTO == null ||requestDTO.Email == null ||
					requestDTO.FromDate == null ||requestDTO.ToDate == null ||
					requestDTO.CreditAccount == null
					) return BadRequest();

				DateTime fromDate;
				DateTime toDate;
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

				if (!Constants.WITHDRAW_TRANSACTION_STATUS.Contains(requestDTO.Status) &&
					requestDTO.Status != Constants.WITHDRAW_TRANSACTION_ALL)
				{
					status.Message = "Invalid transaction's status";
					status.Ok = false;
					status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
					responseData.Status = status;
					return Ok(responseData);
				}

				long withdrawTransactionId;
				long.TryParse(requestDTO.WithdrawTransactionId, out withdrawTransactionId);

				var deposits = bankRepository.GetAllWithdrawTransaction(withdrawTransactionId, requestDTO.Email, fromDate, toDate,requestDTO.BankId,requestDTO.CreditAccount, requestDTO.Status);

				var result = mapper.Map<List<HistoryWithdrawDetail>>(deposits);

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

		#region Cancel withdraw transaction of user
		[Authorize]
		[HttpPut("CancelWithdraw/{id}")]
		public IActionResult CancelWithdrawTransaction(int id)
		{
			try 
			{
				if (!ModelState.IsValid) return BadRequest();
				
				var withdrawTransaction = bankRepository.GetWithdrawTransaction(id);
				if(withdrawTransaction == null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "not found", false, new { }));
				}
				if(withdrawTransaction.WithdrawTransactionStatusId != Constants.WITHDRAW_TRANSACTION_IN_PROCESSING)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_BANK_CUSTOMER_REQUEST_WITHDRAW_CHANGED_BEFORE, "status changed", false, new { }));
				}

				bankRepository.UpdateWithdrawTransactionCancel(id);

				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", false, new {}));
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Get withdraw transaction bill for user
		[Authorize]
		[HttpPost("WithdrawTransactionBill")]
		public IActionResult GetWithdrawTransactionBill(WithdrawTransactionBillRequestDTO requestDTO)
		{
			ResponseData responseData = new ResponseData();
			Status status = new Status();
			try
			{
				if (requestDTO.UserId == 0 || requestDTO.WithdrawTransactionId == 0) return BadRequest();
				var withdrawTransaction = bankRepository.GetWithdrawTransaction(requestDTO.WithdrawTransactionId);

				if(withdrawTransaction == null)
				{
					status.Message = "Withdraw bill not found!";
					status.Ok = false;
					status.ResponseCode = Constants.RESPONSE_CODE_DATA_NOT_FOUND;
					responseData.Status = status;
					return Ok(responseData);
				}

				if (withdrawTransaction.UserId != requestDTO.UserId)
				{
					status.Message = "You not have permitsion to view this data!";
					status.Ok = false;
					status.ResponseCode = Constants.RESPONSE_CODE_UN_AUTHORIZE;
					responseData.Status = status;
					return Ok(responseData);
				}

				if (withdrawTransaction.WithdrawTransactionStatusId == Constants.WITHDRAW_TRANSACTION_IN_PROCESSING)
				{
					status.Message = "Withdraw transaction hasn't paid!";
					status.Ok = false;
					status.ResponseCode = Constants.RESPONSE_CODE_BANK_WITHDRAW_UNPAY;
					responseData.Status = status;
					return Ok(responseData);
				}

				if (withdrawTransaction.WithdrawTransactionStatusId == Constants.WITHDRAW_TRANSACTION_REJECT)
				{
					status.Message = "Withdraw transaction hasn been rejected!";
					status.Ok = false;
					status.ResponseCode = Constants.RESPONSE_CODE_BANK_WITHDRAW_REJECT;
					responseData.Status = status;
					responseData.Result = withdrawTransaction.Note ?? string.Empty;
					return Ok(responseData);
				}

				var bill = bankRepository.GetWithdrawTransactionBill(requestDTO.WithdrawTransactionId);
				if(bill == null)
				{
					status.Message = "Withdraw bill has been in process in partner bank!";
					status.Ok = false;
					status.ResponseCode = Constants.RESPONSE_CODE_BANK_WITHDRAW_BILL_NOT_FOUND;
					responseData.Status = status;
					return Ok(responseData);
				}

				var result = mapper.Map<WithdrawTransactionBillDTO>(bill);
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

		#region Get withdraw transaction bill for admin
		[Authorize(Roles ="Admin")]
		[HttpPost("WithdrawTransactionBillAdmin")]
		public IActionResult GetWithdrawTransactionBillForAdmin(WithdrawTransactionBillRequestDTO requestDTO)
		{
			ResponseData responseData = new ResponseData();
			Status status = new Status();
			try
			{
				if (requestDTO.UserId == 0 || requestDTO.WithdrawTransactionId == 0) return BadRequest();
				var withdrawTransaction = bankRepository.GetWithdrawTransaction(requestDTO.WithdrawTransactionId);

				if (withdrawTransaction == null)
				{
					status.Message = "Withdraw bill not found!";
					status.Ok = false;
					status.ResponseCode = Constants.RESPONSE_CODE_DATA_NOT_FOUND;
					responseData.Status = status;
					return Ok(responseData);
				}


				if (withdrawTransaction.WithdrawTransactionStatusId == Constants.WITHDRAW_TRANSACTION_IN_PROCESSING)
				{
					status.Message = "Withdraw transaction hasn't paid!";
					status.Ok = false;
					status.ResponseCode = Constants.RESPONSE_CODE_BANK_WITHDRAW_UNPAY;
					responseData.Status = status;
					return Ok(responseData);
				}

				if (withdrawTransaction.WithdrawTransactionStatusId == Constants.WITHDRAW_TRANSACTION_REJECT)
				{
					status.Message = "Withdraw transaction hasn been rejected!";
					status.Ok = false;
					status.ResponseCode = Constants.RESPONSE_CODE_BANK_WITHDRAW_REJECT;
					responseData.Result = withdrawTransaction.Note ?? string.Empty;
					responseData.Status = status;
					return Ok(responseData);
				}

				var bill = bankRepository.GetWithdrawTransactionBill(requestDTO.WithdrawTransactionId);
				if (bill == null)
				{
					status.Message = "Withdraw bill has been in process in partner bank!";
					status.Ok = false;
					status.ResponseCode = Constants.RESPONSE_CODE_BANK_WITHDRAW_BILL_NOT_FOUND;
					responseData.Status = status;
					return Ok(responseData);
				}

				var result = mapper.Map<WithdrawTransactionBillDTO>(bill);
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

		#region Confirm transfer withdraw success
		[Authorize(Roles ="Admin")]		
		[HttpPost("ConfirmTransfer")]
		public IActionResult ConfirmTransferWithdrawSuccess(ConfirmTransferWithdrawSuccessRequestDTO requestDTO)
		{
			ResponseData responseData = new ResponseData();
			Status status = new Status();
			try
			{
				if (requestDTO.Id == 0) return BadRequest();
				var withdrawTransaction = bankRepository.GetWithdrawTransaction(requestDTO.Id);
				if (withdrawTransaction == null)
				{
					status.Message = "Withdraw bill not found!";
					status.Ok = false;
					status.ResponseCode = Constants.RESPONSE_CODE_DATA_NOT_FOUND;
					responseData.Status = status;
					return Ok(responseData);
				}
				if (withdrawTransaction.WithdrawTransactionStatusId == Constants.WITHDRAW_TRANSACTION_PAID)
				{
					status.Message = "Withdraw transaction has been paid!";
					status.Ok = false;
					status.ResponseCode = Constants.RESPONSE_CODE_BANK_WITHDRAW_PAID;
					responseData.Status = status;
					return Ok(responseData);
				}

				bankRepository.UpdateWithdrawTransactionPaid(requestDTO.Id);
				status.Message = "Success!";
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

		#region Confirm transfer list withdraw success
		[Authorize(Roles = "Admin")]
		[HttpPost("ConfirmListTransfer")]
		public IActionResult ConfirmTransferListWithdrawSuccess(ConfirmTransferListWithdrawSuccessRequestDTO requestDTO)
		{
			ResponseData responseData = new ResponseData();
			Status status = new Status();
			try
			{
				if (requestDTO.Ids.Count == 0) return BadRequest();
				
				var responeCode = bankRepository.UpdateListWithdrawTransactionPaid(requestDTO.Ids);

				if(responeCode == Constants.RESPONSE_CODE_DATA_NOT_FOUND) 
				{
					status.Message = "A withdraw transaction not found!";
					status.Ok = false;
					status.ResponseCode = Constants.RESPONSE_CODE_DATA_NOT_FOUND;
					responseData.Status = status;
					return Ok(responseData);
				}
				if (responeCode == Constants.RESPONSE_CODE_BANK_WITHDRAW_PAID)
				{
					status.Message = "A withdraw has been paid!";
					status.Ok = false;
					status.ResponseCode = Constants.RESPONSE_CODE_BANK_WITHDRAW_PAID;
					responseData.Status = status;
					return Ok(responseData);
				}

				if (responeCode == Constants.RESPONSE_CODE_FAILD)
				{
					status.Message = "Sever err!";
					status.Ok = false;
					status.ResponseCode = Constants.RESPONSE_CODE_FAILD;
					responseData.Status = status;
					return Ok(responseData);
				}

				status.Message = "Success!";
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

		#region RejectWithdrawTransaction
		[Authorize(Roles = "Admin")]
		[HttpPost("RejectWithdrawTransaction")]
		public IActionResult RejectWithdrawTransaction(RejectWithdrawTransaction requestDTO)
		{
			if (!ModelState.IsValid) return BadRequest();
			ResponseData responseData = new ResponseData();
			Status status = new Status();

			try
			{
				var withdrawTransaction = bankRepository.GetWithdrawTransaction(requestDTO.WithdrawTransactionId);

				if (withdrawTransaction == null)
				{
					status.Message = "Withdraw transaction not existed!";
					status.Ok = false;
					status.ResponseCode = Constants.RESPONSE_CODE_DATA_NOT_FOUND;
					responseData.Status = status;
					return Ok(responseData);
				}

				if (withdrawTransaction.WithdrawTransactionStatusId == Constants.WITHDRAW_TRANSACTION_PAID ||
					withdrawTransaction.WithdrawTransactionStatusId == Constants.WITHDRAW_TRANSACTION_REJECT)
				{
					status.Message = $"Withdraw transaction has been " +
						$"{(withdrawTransaction.WithdrawTransactionId == Constants.WITHDRAW_TRANSACTION_PAID ? "Paid" : "Rejected")}!";
					status.Ok = false;
					status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
					responseData.Status = status;
					return Ok(responseData);
				}

				bankRepository.RejectWithdrawTransaction(requestDTO.WithdrawTransactionId, requestDTO.Note);

				status.Message = "Success!";
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
	}
}
