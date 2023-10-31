using AutoMapper;
using Azure;
using Azure.Core;
using BusinessObject.Entities;
using Comons;
using DataAccess.IRepositories;
using DataAccess.Repositories;
using DigitalFUHubApi.Comons;
using DigitalFUHubApi.Services;
using DTOs.Admin;
using DTOs.Cart;
using DTOs.Order;
using DTOs.Seller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace DigitalFUHubApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class OrdersController : ControllerBase
	{
		private readonly IOrderRepository _orderRepository;
		private readonly JwtTokenService _jwtTokenService;
		private readonly IMapper _mapper;
		private readonly HubService _hubService;

		public OrdersController(IOrderRepository orderRepository, JwtTokenService jwtTokenService, HubService hubService, IMapper mapper)
		{
			_orderRepository = orderRepository;
			_jwtTokenService = jwtTokenService;
			_hubService = hubService;
			_mapper = mapper;
		}

		#region Add order customer
		//[Authorize("Customer,Seller")]
		[HttpPost("Customer/AddOrder")]
		public async Task<IActionResult> AddOrder(AddOrderRequestDTO request)
		{
			try
			{
				ResponseData responseData = new ResponseData();
				if (request.UserId != _jwtTokenService.GetUserIdByAccessToken(User))
				{
					return Unauthorized();
				}
				if (!ModelState.IsValid)
				{
					return BadRequest();
				}
				(string responseCode, string message, int numberQuantityAvailable, Order orderInfo) =
					_orderRepository.AddOrder(request.UserId, request.ShopProducts, request.IsUseCoin);

				responseData.Status.ResponseCode = responseCode;
				responseData.Status.Ok = responseCode == Constants.RESPONSE_CODE_SUCCESS;
				responseData.Status.Message = message;

				if (responseCode == Constants.RESPONSE_CODE_ORDER_NOT_ENOUGH_QUANTITY)
				{
					responseData.Result = numberQuantityAvailable;
				}

				if (responseCode == Constants.RESPONSE_CODE_SUCCESS)
				{
					// send notification
					var title = "Mua hàng thành công";
					var content = $"Mã đơn số {orderInfo.OrderId} đã mua thành công với tổng giá trị đơn hàng {orderInfo.TotalPayment}đ";
					var link = Constants.FRONT_END_HISTORY_ORDER_URL + orderInfo.OrderId;
					await _hubService.SendNotification(request.UserId, title, content, link);
				}

				return Ok(responseData);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}
		#endregion

		#region Get all order of customer
		[Authorize("Customer,Seller")]
		[HttpPost("Customer/List")]
		public IActionResult GetListOrders([FromBody] GetAllOrderRequestDTO request)
		{

			try
			{
				if (request.UserId != _jwtTokenService.GetUserIdByAccessToken(User))
				{
					return Unauthorized();
				}
				if (!ModelState.IsValid)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_FAILD, "INVALID", false, new()));
				}
				List<Order> orders = _orderRepository.GetAllOrderByUser(request.UserId, request.StatusId, request.Limit, request.Offset);
				OrderResponseDTO orderResponse = new OrderResponseDTO()
				{
					NextOffset = orders.Count < request.Limit ? -1 : request.Offset + orders.Count,

					Orders = orders.Select(x => new OrderProductResponseDTO
					{
						OrderId = x.OrderId,
						Note = x.Note ?? "",
						OrderDate = x.OrderDate,
						ShopId = x.ShopId,
						ShopName = x.Shop.ShopName,
						StatusId = x.OrderStatusId,
						TotalAmount = x.TotalAmount,
						TotalCoinDiscount = x.TotalCoinDiscount,
						TotalCouponDiscount = x.TotalCouponDiscount,
						TotalPayment = x.TotalPayment,
						OrderDetails = x.OrderDetails.Select(od => new OrderDetailProductResponseDTO
						{
							Discount = od.Discount,
							IsFeedback = od.IsFeedback,
							OrderDetailId = od.OrderDetailId,
							Price = od.Price,
							ProductId = od.ProductVariant.ProductId,
							ProductName = od.ProductVariant?.Product?.ProductName ?? "",
							ProductVariantId = od.ProductVariantId,
							ProductVariantName = od.ProductVariant?.Name ?? "",
							Quantity = od.Quantity,
							Thumbnail = od.ProductVariant?.Product?.Thumbnail ?? "",
							TotalAmount = od.TotalAmount
						}).ToList(),
					}).ToList()

				};
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "SUCCESS", true, orderResponse));
			}
			catch (Exception e)
			{

				return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
			}

		}
		#endregion

		#region Update status order customer
		[Authorize("Customer,Seller")]
		[HttpPost("Customer/Edit/Status")]
		public async Task<IActionResult> UpdateStatusOrder([FromBody] EditStatusOrderRequestDTO request)
		{
			try
			{
				if (request.UserId != _jwtTokenService.GetUserIdByAccessToken(User))
				{
					return Unauthorized();
				}
				if (!ModelState.IsValid)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_FAILD, "INVALID", false, new()));
				}
				Order? order = _orderRepository.GetOrderCustomer(request.OrderId, request.UserId, request.ShopId);
				if (order == null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "NOT FOUND", false, new()));
				}
				else if (request.StatusId != Constants.ORDER_COMPLAINT && request.StatusId != Constants.ORDER_CONFIRMED)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_FAILD, "INVALID STATUS ORDER", false, new()));
				}

				_orderRepository.UpdateOrderStatusCustomer(request.OrderId, request.ShopId, request.StatusId);

				string title = $"{(request.StatusId == Constants.ORDER_CONFIRMED ? "Xác nhận đơn hàng thành công." : "Đơn hàng đang được khiếu nại.")}";
				string content = $"Mã đơn số {request.OrderId} {(request.StatusId == Constants.ORDER_CONFIRMED ? "đã được xác nhận." : "đang khiếu nại.")}";
				string link = Constants.FRONT_END_HISTORY_ORDER_URL + request.OrderId;
				await _hubService.SendNotification(request.UserId, title, content, link);
			}
			catch (Exception e)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
			}
			return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "SUCCESS", true, new()));
		}
		#endregion

		#region Get order detail of customer
		[Authorize("Customer,Seller")]
		[HttpGet("Customer/{userId}/{orderId}")]
		public IActionResult GetOrderDetailCustomer(long userId, long orderId)
		{
			try
			{
				if (userId != _jwtTokenService.GetUserIdByAccessToken(User))
				{
					return Unauthorized();
				}

				Order? order = _orderRepository.GetOrderCustomer(orderId, userId);
				if (order == null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "NOT FOUND", false, new()));

				}
				OrderProductResponseDTO responseData = new OrderProductResponseDTO
				{
					OrderId = order.OrderId,
					Note = order.Note ?? "",
					OrderDate = order.OrderDate,
					ShopId = order.ShopId,
					ShopName = order.Shop.ShopName,
					ConversationId = order.ConversationId ?? 0,
					StatusId = order.OrderStatusId,
					TotalAmount = order.TotalAmount,
					TotalCoinDiscount = order.TotalCoinDiscount,
					TotalCouponDiscount = order.TotalCouponDiscount,
					TotalPayment = order.TotalPayment,
					OrderDetails = order.OrderDetails.Select(od => new OrderDetailProductResponseDTO
					{
						Discount = od.Discount,
						IsFeedback = od.IsFeedback,
						OrderDetailId = od.OrderDetailId,
						Price = od.Price,
						ProductId = od.ProductVariant.ProductId,
						ProductName = od.ProductVariant?.Product?.ProductName ?? "",
						ProductVariantId = od.ProductVariantId,
						ProductVariantName = od.ProductVariant?.Name ?? "",
						Quantity = od.Quantity,
						Thumbnail = od.ProductVariant?.Product?.Thumbnail ?? "",
						TotalAmount = od.TotalAmount,
						AssetInformations = od.AssetInformations.Select(x => x.Asset ?? "").ToList(),
						FeebackRate = od?.Feedback?.Rate ?? 0
					}).ToList(),
				};
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "SUCCESS", true, responseData));
			}
			catch (Exception e)
			{

				return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
			}

		}
		#endregion

		#region Get list orders seller
		[Authorize("Seller")]
		[HttpPost("Seller/List")]
		public IActionResult GetOrdersSeller(SellerOrdersRequestDTO request)
		{
			try
			{
				if (request.UserId != _jwtTokenService.GetUserIdByAccessToken(User))
				{
					return Unauthorized();
				}
				if (request == null || request.OrderId == null ||
					request.Username == null
					|| (string.IsNullOrWhiteSpace(request.FromDate) && !string.IsNullOrWhiteSpace(request.FromDate))
					|| (!string.IsNullOrWhiteSpace(request.FromDate) && string.IsNullOrWhiteSpace(request.FromDate)))
				{
					return BadRequest();
				}

				int[] acceptedOrderStatus = Constants.ORDER_STATUS;
				if (!acceptedOrderStatus.Contains(request.Status) && request.Status != Constants.ORDER_ALL)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "INVALID STATUS ORDER", false, new()));
				}

				DateTime? fromDate;
				DateTime? toDate;
				string format = "M/d/yyyy";

				fromDate = string.IsNullOrWhiteSpace(request.FromDate) ? null : DateTime.ParseExact(request.FromDate, format, CultureInfo.InvariantCulture);
				toDate = string.IsNullOrWhiteSpace(request.ToDate) ? null : DateTime.ParseExact(request.ToDate, format, CultureInfo.InvariantCulture).AddDays(1);
				if (fromDate > toDate && fromDate != null && toDate != null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "INVALID DATE", false, new()));
				}

				long orderId;
				long.TryParse(request.OrderId, out orderId);
				List<Order> orders = _orderRepository.GetListOrderSeller(request.UserId, orderId, request.Username.Trim(), fromDate, toDate, request.Status);
				List<SellerOrderResponseDTO> result = _mapper.Map<List<SellerOrderResponseDTO>>(orders);

				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "SUCCESS", true, result));
			}
			catch (Exception e)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
			}
		}
		#endregion

		#region Get order detail seller
		[Authorize("Seller")]
		[HttpGet("Seller/{userId}/{orderId}")]
		public IActionResult GetOrderDetailSeller(long userId, long orderId)
		{
			if (userId != _jwtTokenService.GetUserIdByAccessToken(User))
			{
				return Unauthorized();
			}
			Order? order = _orderRepository.GetOrderDetailSeller(userId, orderId);
			if (order == null)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "NOT FOUND", false, new()));
			}

			SellerOrderDetailResponseDTO response = new SellerOrderDetailResponseDTO
			{
				OrderId = order.OrderId,
				Note = order.Note ?? "",
				OrderDate = order.OrderDate,
				ShopId = order.ShopId,
				CustomerId = order.User.UserId,
				CustomerUsername = order.User.Username,
				StatusId = order.OrderStatusId,
				TotalAmount = order.TotalAmount,
				TotalCoinDiscount = order.TotalCoinDiscount,
				TotalCouponDiscount = order.TotalCouponDiscount,
				TotalPayment = order.TotalPayment,
				OrderDetails = order.OrderDetails.Select(od => new SellerOrderDetailProductResponseDTO
				{
					Discount = od.Discount,
					OrderDetailId = od.OrderDetailId,
					Price = od.Price,
					ProductId = od.ProductVariant.ProductId,
					ProductName = od.ProductVariant?.Product?.ProductName ?? "",
					ProductVariantId = od.ProductVariantId,
					ProductVariantName = od.ProductVariant?.Name ?? "",
					Quantity = od.Quantity,
					Thumbnail = od.ProductVariant?.Product?.Thumbnail ?? "",
					TotalAmount = od.TotalAmount,
				}).ToList(),
			};
			return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "SUCCESS", true, response));
		}
		#endregion

		#region Seller dispute order
		[Authorize("Seller")]		
		[HttpPost("Seller/Dispute")]		
		public IActionResult UpdateDisputeOrder(SellerDisputeOrderRequestDTO request) 
		{
			if(!ModelState.IsValid)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "INVALID", false, new()));
			}
			try
			{
				if(request.SellerId != _jwtTokenService.GetUserIdByAccessToken(User))
				{
					return Unauthorized();
				}
				_orderRepository.UpdateStatusOrderDispute(request.SellerId, request.CustomerId, request.OrderId);
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "SUCCESS", true, new()));
			}
			catch (Exception e)
			{

				return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
			}
		}
		#endregion

		#region Seller dispute order
		[Authorize("Seller")]
		[HttpPost("Seller/Refund")]
		public IActionResult UpdateRefundOrder(SellerRefundOrderRequestDTO request)
		{
			if (!ModelState.IsValid || string.IsNullOrWhiteSpace(request.Note))
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "INVALID", false, new()));
			}
			try
			{
				if (request.SellerId != _jwtTokenService.GetUserIdByAccessToken(User))
				{
					return Unauthorized();
				}
				_orderRepository.UpdateStatusOrderRefund(request.SellerId, request.OrderId, request.Note.Trim());
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "SUCCESS", true, new()));
			}
			catch (Exception e)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
			}
		}
		#endregion

		#region Get list orders admin
		[Authorize("Admin")]
		[HttpPost("Admin/All")]
		public IActionResult GetOrders(OrdersRequestDTO requestDTO)
		{
			if (!ModelState.IsValid) return BadRequest();

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

				var orders = _orderRepository.GetOrders(orderId, requestDTO.CustomerEmail, requestDTO.ShopName, fromDate, toDate, requestDTO.Status);
				var result = _mapper.Map<List<OrdersResponseDTO>>(orders);

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

		#region Get order detail admin
		[Authorize("Admin")]
		[HttpPost("Admin/GetOrder/{id}")]
		public IActionResult GetOrder(int id)
		{
			if (!ModelState.IsValid) return BadRequest();

			ResponseData responseData = new ResponseData();
			Status status = new Status();

			try
			{
				var order = _orderRepository.GetOrder(id);
				if (order == null)
				{
					status.Message = "Order not existed!";
					status.Ok = false;
					status.ResponseCode = Constants.RESPONSE_CODE_DATA_NOT_FOUND;
					responseData.Status = status;
					return Ok(responseData);
				}

				var result = _mapper.Map<OrderInfoResponseDTO>(order);

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

		#region Update order status admin
		[Authorize("Admin")]
		[HttpPost("Admin/UpdateOrderStatus")]
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

				var order = _orderRepository.GetOrderForCheckingExisted(requestDTO.OrderId);
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
					_orderRepository.UpdateOrderStatusRejectComplaint(requestDTO.OrderId, requestDTO.Note);
				}
				else if (requestDTO.Status == Constants.ORDER_SELLER_VIOLATES)
				{
					_orderRepository.UpdateOrderStatusSellerViolates(requestDTO.OrderId, requestDTO.Note);
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
	}


}
