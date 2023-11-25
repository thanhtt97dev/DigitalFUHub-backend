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
using OfficeOpenXml.Style;

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
			try
			{
				if (id == 0) return BadRequest();

				var user = userRepository.GetUserById(id);
				if (user == null) return BadRequest();

				var bank = bankRepository.GetUserBank(id);
				if (bank == null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Not found user's bank account!", false, new()));
				}

				var result = mapper.Map<BankAccountResponeDTO>(bank);
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success!", true, result));
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
					return Ok(new ResponseData(Constants.RESPONSE_CODE_FAILD, "Request many time in same time!", false, new()));
				}
				else if (mbBankResponse.Code == Constants.MB_BANK_RESPONE_CODE_SESSION_INVALID)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_FAILD, "Third-party's session invalid", false, new()));
				}
				else if (mbBankResponse.Code == Constants.MB_BANK_RESPONE_CODE_SEARCH_WITH_TYPE_ERR)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_FAILD, "Search data err", false, new()));
				}
				else if (mbBankResponse.Code == Constants.MB_BANK_RESPONE_CODE_ACCOUNT_NOT_FOUND)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Not found", false, new()));
				}
				/*
				else if (mbBankResponse.Code == Constants.MB_BANK_RESPONE_CODE_SUCCESS)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", false, mbBankResponse.Result));
				}
				*/
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", false, mbBankResponse.Result));
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
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "After few days can update", false, new()));
				}

				BankInquiryAccountNameRequestDTO bankInquiryAccount = new BankInquiryAccountNameRequestDTO()
				{
					BankId = bankLinkAccountRequestDTO.BankId,
					CreditAccount = bankLinkAccountRequestDTO.CreditAccount,
				};

				var mbBank = await mbBankService.InquiryAccountName(bankInquiryAccount);
				if (mbBank == null || mbBank.Result == null) //RULE2 2
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_FAILD, "Third-party's server err", false, new()));
				}
				else
				{
					if (mbBank.Code != Constants.MB_BANK_RESPONE_CODE_SUCCESS)
					{
						if (mbBank.Code == Constants.MB_BANK_RESPONE_CODE_ACCOUNT_NOT_FOUND) //RULE 3
						{
							return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Not found", false, new()));
						}
						else //RULE 2
						{
							return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Third-party's server err", false, new()));
						}
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

				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Update user's bank account success!", true, new()));
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
			try
			{
				if (id == 0 || request == null || request.FromDate == null ||request.ToDate == null) return BadRequest();

				(bool isValid, DateTime? fromDate, DateTime? toDate) = Util.GetFromDateToDate(request.FromDate, request.ToDate);
				if (!isValid)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid date", false, new()));
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
			try
			{
				(bool isValid, DateTime? fromDate, DateTime? toDate) = Util.GetFromDateToDate(request.FromDate, request.ToDate);
				if (!isValid)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid date", false, new()));
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
			try
			{
				if (id == 0 || request == null || request.FromDate == null || request.ToDate == null) return BadRequest();

				if (!Constants.WITHDRAW_TRANSACTION_STATUS.Contains(request.Status) &&
					request.Status != Constants.WITHDRAW_TRANSACTION_ALL)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid transaction's status", false, new()));
				}

				(bool isValid, DateTime? fromDate, DateTime? toDate) = Util.GetFromDateToDate(request.FromDate, request.ToDate);
				if (!isValid)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid date", false, new()));
				}

				long withdrawTransactionId;
				long.TryParse(request.WithdrawTransactionId, out withdrawTransactionId);

				var totalRecord = bankRepository.GetNumberWithdrawTransactionWithCondition(id, withdrawTransactionId, request.Email, fromDate, toDate, request.BankId, request.CreditAccount, request.Status);
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
		public IActionResult GetHistoryWithdrawTransactionForAdmin(HistoryWithdrawRequestDTO request)
		{
			if (!ModelState.IsValid) return BadRequest();
			try
			{
				(bool isValid, DateTime? fromDate, DateTime? toDate) = Util.GetFromDateToDate(request.FromDate, request.ToDate);
				if (!isValid)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid date", false, new()));
				}

				if (!Constants.WITHDRAW_TRANSACTION_STATUS.Contains(request.Status) && request.Status != Constants.WITHDRAW_TRANSACTION_ALL)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid transaction's status", false, new()));
				}

				long withdrawTransactionId;
				long.TryParse(request.WithdrawTransactionId, out withdrawTransactionId);

				var totalRecord = bankRepository.GetNumberWithdrawTransactionWithCondition(0, withdrawTransactionId, request.Email, fromDate, toDate, request.BankId, request.CreditAccount, request.Status);
				var numberPages = totalRecord / Constants.PAGE_SIZE + 1;
				if (request.Page > numberPages || request.Page == 0)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid number page", false, new()));
				}

				var withdraws = bankRepository.GetAllWithdrawTransaction(withdrawTransactionId, request.Email, fromDate, toDate, request.BankId, request.CreditAccount, request.Status, request.Page);

				var result = new
				{
					Total = totalRecord,
					WithdrawTransactions = mapper.Map<List<HistoryWithdrawDetail>>(withdraws)
				};
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, result));
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Get all withdraw transaction unpay
		[Authorize(Roles = "Admin")]
		[HttpPost("GetAllWithdrawUnPay")]
		public IActionResult GetAllWithdrawUnPay(WithdrawTransactionUnPayRequestDTO request)
		{
			if (!ModelState.IsValid) return BadRequest();
			try
			{
				(bool isValid, DateTime? fromDate, DateTime? toDate) = Util.GetFromDateToDate(request.FromDate, request.ToDate);
				if (!isValid)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid date", false, new()));
				}

				long withdrawTransactionId;
				long.TryParse(request.WithdrawTransactionId, out withdrawTransactionId);

				var withdraws = bankRepository.GetAllWithdrawTransactionUnPay(withdrawTransactionId, request.Email, fromDate, toDate, request.BankId, request.CreditAccount);
				var result = mapper.Map<List<WithdrawTransactionUnpayResponseDTO  >>(withdraws);
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, result));
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
			try
			{
				if (requestDTO.UserId == 0 || requestDTO.WithdrawTransactionId == 0) return BadRequest();
				var withdrawTransaction = bankRepository.GetWithdrawTransaction(requestDTO.WithdrawTransactionId);

				if(withdrawTransaction == null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Withdraw bill not found!", false, new()));
				}

				if (withdrawTransaction.UserId != requestDTO.UserId)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_UN_AUTHORIZE, "You not have permitsion to view this data!", false, new()));
				}

				if (withdrawTransaction.WithdrawTransactionStatusId == Constants.WITHDRAW_TRANSACTION_IN_PROCESSING)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_BANK_WITHDRAW_UNPAY, "Withdraw transaction hasn't paid!", false, new()));
				}

				if (withdrawTransaction.WithdrawTransactionStatusId == Constants.WITHDRAW_TRANSACTION_REJECT)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_BANK_WITHDRAW_REJECT, "Withdraw transaction hasn been rejected!", false, withdrawTransaction.Note ?? string.Empty));
				}

				var bill = bankRepository.GetWithdrawTransactionBill(requestDTO.WithdrawTransactionId);
				if(bill == null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_BANK_WITHDRAW_BILL_NOT_FOUND, "Withdraw bill has been in process in partner bank!", false, new()));
				}

				var result = mapper.Map<WithdrawTransactionBillDTO>(bill);
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success!", true, result));
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
			try
			{
				if (requestDTO.UserId == 0 || requestDTO.WithdrawTransactionId == 0) return BadRequest();
				var withdrawTransaction = bankRepository.GetWithdrawTransaction(requestDTO.WithdrawTransactionId);

				if (withdrawTransaction == null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Withdraw bill not found!", false, new()));
				}

				if (withdrawTransaction.WithdrawTransactionStatusId == Constants.WITHDRAW_TRANSACTION_IN_PROCESSING)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_BANK_WITHDRAW_UNPAY, "Withdraw transaction hasn't paid!", false, new()));
				}

				if (withdrawTransaction.WithdrawTransactionStatusId == Constants.WITHDRAW_TRANSACTION_REJECT)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_BANK_WITHDRAW_REJECT, "Withdraw transaction hasn been rejected!", false, withdrawTransaction.Note ?? string.Empty));
				}

				var bill = bankRepository.GetWithdrawTransactionBill(requestDTO.WithdrawTransactionId);
				if (bill == null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_BANK_WITHDRAW_BILL_NOT_FOUND, "Withdraw bill has been in process in partner bank!", false, new()));
				}

				var result = mapper.Map<WithdrawTransactionBillDTO>(bill);
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success!", true, result));
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
			try
			{
				if (requestDTO.Id == 0) return BadRequest();
				var withdrawTransaction = bankRepository.GetWithdrawTransaction(requestDTO.Id);
				if (withdrawTransaction == null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Withdraw bill not found!", false, new()));
				}
				if (withdrawTransaction.WithdrawTransactionStatusId == Constants.WITHDRAW_TRANSACTION_PAID)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_BANK_WITHDRAW_PAID, "Withdraw transaction has been paid!", false, new()));
				}

				bankRepository.UpdateWithdrawTransactionPaid(requestDTO.Id);
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, new()));
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
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "A withdraw transaction not found!", false, new()));
				}
				if (responeCode == Constants.RESPONSE_CODE_BANK_WITHDRAW_PAID)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_BANK_WITHDRAW_PAID, "A withdraw has been paid!", false, new()));
				}

				if (responeCode == Constants.RESPONSE_CODE_FAILD)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_FAILD, "Sever err!", false, new()));
				}

				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success!", true, new()));
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
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Withdraw transaction not existed!", false, new()));
				}

				if (withdrawTransaction.WithdrawTransactionStatusId == Constants.WITHDRAW_TRANSACTION_PAID ||
					withdrawTransaction.WithdrawTransactionStatusId == Constants.WITHDRAW_TRANSACTION_REJECT)
				{
					
					var message = $"Withdraw transaction has been " +
						$"{(withdrawTransaction.WithdrawTransactionId == Constants.WITHDRAW_TRANSACTION_PAID ? "Paid" : "Rejected")}!";
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, message, false, new()));
				}

				bankRepository.RejectWithdrawTransaction(requestDTO.WithdrawTransactionId, requestDTO.Note);

				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success!", true, new()));
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion
	}
}
