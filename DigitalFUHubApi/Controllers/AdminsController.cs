using AutoMapper;
using BusinessObject.Entities;
using Comons;
using DataAccess.IRepositories;
using DataAccess.Repositories;
using DigitalFUHubApi.Comons;
using DigitalFUHubApi.Services;
using DTOs.Admin;
using DTOs.Bank;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DigitalFUHubApi.Controllers
{
	//[Authorize(Roles ="Admin")]
	[Route("api/[controller]")]
	[ApiController]
	public class AdminsController : ControllerBase
	{
		private readonly IMapper mapper;
		private readonly IOrderRepository orderRepository;
		private readonly IBankRepository bankRepository;
		private readonly IUserRepository userRepository;
		private readonly IBusinessFeeRepository businessFeeRepository;
		private readonly MailService mailService;

		public AdminsController(IMapper mapper, IOrderRepository orderRepository, IBankRepository bankRepository, IUserRepository userRepository, IBusinessFeeRepository businessFeeRepository, MailService mailService)
		{
			this.mapper = mapper;
			this.orderRepository = orderRepository;
			this.bankRepository = bankRepository;
			this.userRepository = userRepository;
			this.businessFeeRepository = businessFeeRepository;
			this.mailService = mailService;
		}

		#region Get orders
		[HttpPost("GetOrders")]
		public IActionResult GetOrders(OrdersRequestDTO requestDTO)
		{
			if (requestDTO == null || requestDTO.OrderId == null ||
				requestDTO.CustomerEmail == null || requestDTO.ShopName == null ||
				requestDTO.ToDate == null || requestDTO.FromDate == null) return BadRequest();

			ResponseData responseData = new ResponseData();
			Status status = new Status();

			int[] acceptedOrderStatus = Constants.ORDER_STATUS;
			if (!acceptedOrderStatus.Contains(requestDTO.Status) && requestDTO.Status != Constants.ORDER_ALL)
			{
				status.Message = "Invalid order status!";
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

				var orders = orderRepository.GetOrders(orderId, requestDTO.CustomerEmail, requestDTO.ShopName, fromDate, toDate, requestDTO.Status);
				var result = mapper.Map<List<OrdersResponseDTO>>(orders);

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

		#region Get order
		[HttpPost("GetOrder/{id}")]
		public IActionResult GetOrder(int id)
		{
			if (id == 0) return BadRequest();
			ResponseData responseData = new ResponseData();
			Status status = new Status();
			
			try
			{
				var order = orderRepository.GetOrder(id);
				if(order == null)
				{
					status.Message = "Order not existed!";
					status.Ok = false;
					status.ResponseCode = Constants.RESPONSE_CODE_DATA_NOT_FOUND;
					responseData.Status = status;
					return Ok(responseData);
				}

				var result = mapper.Map<OrderDetailResponseDTO>(order);	

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

		#region Update order status
		[HttpPost("UpdateOrderStatus")]
		public IActionResult UpdateOrderStatus(UpdateOrderStatusRequestDTO requestDTO)
		{
			if (requestDTO.OrderId == 0 || requestDTO.Status == 0) return BadRequest();
			ResponseData responseData = new ResponseData();
			Status status = new Status();

			try
			{
				int[] statusAccepted = { Constants.ORDER_DISPUTE, Constants.ORDER_REJECT_COMPLAINT, Constants.ORDER_SELLER_VIOLATES };
				if (!statusAccepted.Contains(requestDTO.Status))
				{
					status.Message = "Invalid order status!";
					status.Ok = false;
					status.ResponseCode = Constants.RESPONSE_CODE_DATA_NOT_FOUND;
					responseData.Status = status;
					return Ok(responseData);
				}

				var order = orderRepository.GetOrderForCheckingExisted(requestDTO.OrderId);
				if (order == null)
				{
					status.Message = "Order not existed!";
					status.Ok = false;
					status.ResponseCode = Constants.RESPONSE_CODE_DATA_NOT_FOUND;
					responseData.Status = status;
					return Ok(responseData);
				}

				if (requestDTO.Status == Constants.ORDER_REJECT_COMPLAINT)
				{
					orderRepository.UpdateOrderStatusRejectComplaint(requestDTO.OrderId, requestDTO.Note);
				}
				else if (requestDTO.Status == Constants.ORDER_SELLER_VIOLATES)
				{
					orderRepository.UpdateOrderStatusSellerViolates(requestDTO.OrderId, requestDTO.Note);
				}
				else
				{
					status.Message = "Invalid order status!";
					status.Ok = false;
					status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
					responseData.Status = status;
					return Ok(responseData);
				}

				// check seller have VIOLATE 

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
		[HttpPost("RejectWithdrawTransaction")]
		public IActionResult RejectWithdrawTransaction(RejectWithdrawTransaction requestDTO)
		{
			if (requestDTO.WithdrawTransactionId == 0 || string.IsNullOrEmpty(requestDTO.Note)) return BadRequest();
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

				if(withdrawTransaction.WithdrawTransactionStatusId == Constants.WITHDRAW_TRANSACTION_PAID ||
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

		#region Get users
		[HttpPost("GetUsers")]
		public IActionResult GetUsers(UsersRequestDTO requestDTO)
		{
			
			if(requestDTO == null || requestDTO.Email == null ||
				requestDTO.FullName == null || requestDTO.UserId == null)
			{
				return BadRequest();
			}

			ResponseData responseData = new ResponseData();
			Status status = new Status();

			try
			{
				long userId = 0;
				long.TryParse(requestDTO.UserId, out userId);

				var users = userRepository.GetUsers(userId, requestDTO.Email, requestDTO.FullName, requestDTO.RoleId, requestDTO.Status);
				var result = mapper.Map<List<UsersResponseDTO>>(users);

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

		#region Get all business fee
		[HttpPost("GetBusinessFee")]
		public IActionResult GetBusinessFee(BusinessFeeRequestDTO request)
		{
			if (!ModelState.IsValid) return BadRequest();

			ResponseData responseData = new ResponseData();
			try
			{
				DateTime fromDate;
				DateTime toDate;
				string format = "M/d/yyyy";
				try
				{
					fromDate = DateTime.ParseExact(request.FromDate, format, System.Globalization.CultureInfo.InvariantCulture);
					toDate = DateTime.ParseExact(request.ToDate, format, System.Globalization.CultureInfo.InvariantCulture).AddDays(1);
					if (fromDate > toDate)
					{
						responseData.Status.Message = "From date must be less than to date";
						responseData.Status.Ok = false;
						responseData.Status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
						return Ok(responseData);
					}
				}
				catch (FormatException)
				{
					responseData.Status.Message = "Invalid datetime";
					responseData.Status.Ok = false;
					responseData.Status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
					return Ok(responseData);
				}

				long businessFeeId;
				long.TryParse(request.BusinessFeeId, out businessFeeId);	

				var fees = businessFeeRepository.GetBusinessFee(businessFeeId, request.MaxFee, fromDate, toDate);
				responseData.Status.ResponseCode = Constants.RESPONSE_CODE_SUCCESS;
				responseData.Status.Ok = true;
				responseData.Status.Message = "Success!";
				responseData.Result = fees;
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Add new business fee
		[HttpPost("AddNewBusinessFee")]
		public IActionResult AddNewBusinessFee(CreateBusinessFeeRequestDTO request)
		{
			if (!ModelState.IsValid) return BadRequest();

			ResponseData responseData = new ResponseData();
			try
			{
				businessFeeRepository.AddNewBusinessFee(request.Fee);

				responseData.Status.ResponseCode = Constants.RESPONSE_CODE_SUCCESS;
				responseData.Status.Ok = true;
				responseData.Status.Message = "Success!";
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Get user info by Id
		[HttpPost("GetUser/{id}")]
		public IActionResult GetUser(int id)
		{
			if(id == 0) return  BadRequest();	
			ResponseData responseData = new ResponseData();

			try
			{
				

				responseData.Status.ResponseCode = Constants.RESPONSE_CODE_SUCCESS;
				responseData.Status.Ok = true;
				responseData.Status.Message = "Success!";
				
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
 