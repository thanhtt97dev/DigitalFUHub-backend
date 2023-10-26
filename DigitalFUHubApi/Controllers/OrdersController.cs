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
				if (!ModelState.IsValid)
				{
					return BadRequest();
				}

				ResponseData responseData = new ResponseData();
				//var accessToken = Util.GetAccessToken(HttpContext);
				//var userIdFromAccessToken = _jwtTokenService.GetUserIdByAccessToken(accessToken);
				//if (request.UserId != userIdFromAccessToken)
				//{
				//	responseData.Status.ResponseCode = Constants.RESPONSE_CODE_UN_AUTHORIZE;
				//	responseData.Status.Ok = false;
				//	responseData.Status.Message = "Not have permission!";
				//	return Ok(responseData);
				//}

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
		[HttpPost("Customer/All")]
		public IActionResult GetOrders([FromBody] GetAllOrderRequestDTO request)
		{
			if (!ModelState.IsValid)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_FAILD, "INVALID", false, new()));
			}
			try
			{
				long userId = Util.Instance.GetUserId(User);
				List<Order> orders = _orderRepository.GetAllOrderByUser(userId, request.StatusId, request.Limit, request.Offset);
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
			catch (Exception)
			{

				return Ok(new ResponseData(Constants.RESPONSE_CODE_FAILD, "FAIL", false, new()));
			}

		}
		#endregion

		#region Update status order customer
		[Authorize("Customer,Seller")]
		[HttpPost("Customer/Edit/Status")]
		public async Task<IActionResult> UpdateStatusOrder([FromBody] EditStatusOrderRequestDTO request)
		{
			if (!ModelState.IsValid)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_FAILD, "INVALID", false, new()));
			}
			try
			{
				long customerId = Util.Instance.GetUserId(User);
				Order? order = _orderRepository.GetOrderCustomer(request.OrderId, customerId, request.ShopId);
				if (order == null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_FAILD, "INVALID", false, new()));
				}
				else if (request.StatusId != Constants.ORDER_COMPLAINT && request.StatusId != Constants.ORDER_CONFIRMED)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_FAILD, "INVALID", false, new()));
				}

				_orderRepository.UpdateOrderStatusCustomer(request.OrderId, request.ShopId, request.StatusId);

				string title = $"{(request.StatusId == Constants.ORDER_CONFIRMED ? "Xác nhận đơn hàng thành công." : "Đơn hàng đang được khiếu nại.")}";
				string content = $"Mã đơn số {request.OrderId} {(request.StatusId == Constants.ORDER_CONFIRMED ? "đã được xác nhận." : "đang khiếu nại.")}";
				string link = Constants.FRONT_END_HISTORY_ORDER_URL + request.OrderId;
				await _hubService.SendNotification(customerId, title, content, link);
			}
			catch (Exception e)
			{
				return Conflict(e.Message);
			}
			return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "SUCCESS", true, new()));
		}
		#endregion

		#region Get order detail of customer
		[Authorize("Customer,Seller")]
		[HttpGet("Customer/{orderId}")]
		public IActionResult GetOrderDetailCustomer(long orderId)
		{
			try
			{
				long customerId = Util.Instance.GetUserId(User);
				Order? order = _orderRepository.GetOrderCustomer(orderId, customerId);
				if(order == null)
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

				return Ok(new ResponseData(Constants.RESPONSE_CODE_FAILD, e.Message, false, new()));
			}

		}
		#endregion

		#region Get list orders seller
		[Authorize("Seller")]
		[HttpPost("Seller/All")]
		public IActionResult GetOrdersSeller(SellerOrdersRequestDTO request)
		{
			if (request == null || request.OrderId == null ||
				request.CustomerEmail == null ||
				request.ToDate == null || request.FromDate == null) return BadRequest();

			int[] acceptedOrderStatus = Constants.ORDER_STATUS;
			if (!acceptedOrderStatus.Contains(request.Status) && request.Status != Constants.ORDER_ALL)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "INVALID", false, new()));
			}
			try
			{

				DateTime fromDate;
				DateTime toDate;
				string format = "M/d/yyyy";
				try
				{
					fromDate = DateTime.ParseExact(request.FromDate, format, CultureInfo.InvariantCulture);
					toDate = DateTime.ParseExact(request.ToDate, format, CultureInfo.InvariantCulture).AddDays(1);
					if (fromDate > toDate)
					{
						return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "INVALID DATE", false, new()));
					}
				}
				catch (FormatException)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_FAILD, "INVALID DATE", false, new()));
				}
				long orderId;
				long.TryParse(request.OrderId, out orderId);
				long userId = Util.Instance.GetUserId(User);
				List<Order> orders = _orderRepository.GetOrders(orderId, request.CustomerEmail, "",
					fromDate, toDate, request.Status)
					.Where(x => x.Shop.UserId == userId)
					.ToList();
				List<OrdersResponseDTO> result = _mapper.Map<List<OrdersResponseDTO>>(orders);

				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "SUCCESS", true, result));
			}
			catch (Exception e)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_FAILD, e.Message, false, new()));
			}
		}
		#endregion

		#region Get order detail seller
		[Authorize("Seller")]
		[HttpGet("Seller/{orderId}")]
		public IActionResult GetOrderDetailSeller(long orderId)
		{
			ResponseData response = new ResponseData();
			if (orderId == 0)
			{
				return BadRequest();
			}

			Order? orderRaw = _orderRepository.GetSellerOrderDetail(orderId);

			if (orderRaw == null)
			{
				response.Status.Ok = false;
				response.Status.Message = "Not found.";
				response.Status.ResponseCode = Constants.RESPONSE_CODE_DATA_NOT_FOUND;
				return Ok(response);
			}
			SellerOrderDetailResponseDTO order = new SellerOrderDetailResponseDTO
			{
				/*
				EmailCustomer = orderRaw.User.Email,
				IsFeedbacked = orderRaw.IsFeedback,
				OrderDate = orderRaw.OrderDate,
				OrderId = orderId,
				OrderStatusId = orderRaw.OrderStatusId,
				Price = orderRaw.Price,
				Quantity = orderRaw.Quantity,
				ProductName = orderRaw.ProductVariant.Product?.ProductName ?? "",
				ProductVariantName = orderRaw.ProductVariant?.Name ?? "",
				Thumbnail = orderRaw.ProductVariant?.Product?.Thumbnail ?? ""
				*/
			};
			response.Status.Ok = true;
			response.Status.Message = "Success";
			response.Status.ResponseCode = Constants.RESPONSE_CODE_SUCCESS;
			response.Result = order;
			return Ok(response);
		}
		#endregion
	}

}
