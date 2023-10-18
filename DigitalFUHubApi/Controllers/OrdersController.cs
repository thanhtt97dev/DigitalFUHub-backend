using AutoMapper;
using Azure;
using Azure.Core;
using BusinessObject.Entities;
using Comons;
using DataAccess.IRepositories;
using DataAccess.Repositories;
using DigitalFUHubApi.Comons;
using DigitalFUHubApi.Services;
using DTOs.Cart;
using DTOs.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DigitalFUHubApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class OrdersController : ControllerBase
	{
		private readonly IOrderRepository orderRepository;
		private readonly JwtTokenService jwtTokenService;
		private readonly HubService hubService;

		public OrdersController(IOrderRepository orderRepository, JwtTokenService jwtTokenService, HubService hubService)
		{
			this.orderRepository = orderRepository;
			this.jwtTokenService = jwtTokenService;
			this.hubService = hubService;
		}

		//[Authorize("Customer,Seller")]
		[HttpPost("AddOrder")]
		public async Task<IActionResult> AddOrder(AddOrderRequestDTO request)
		{
			try
			{
				if (!ModelState.IsValid)
				{
					return BadRequest();
				}

				ResponseData responseData = new ResponseData();
				var accessToken = Util.GetAccessToken(HttpContext);
				var userIdFromAccessToken = jwtTokenService.GetUserIdByAccessToken(accessToken);
				if (request.UserId != userIdFromAccessToken)
				{
					responseData.Status.ResponseCode = Constants.RESPONSE_CODE_UN_AUTHORIZE;
					responseData.Status.Ok = false;
					responseData.Status.Message = "Not have permission!";
					return Ok(responseData);
				}

				(string responseCode, string message, int numberQuantityAvailable, Order orderInfo) =
					orderRepository.AddOrder(request.UserId, request.ShopProducts, request.IsUseCoin);

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
					var link = Constants.FRONT_END_HISTORY_ORDER_URL;
					await hubService.SendNotification(request.UserId, title, content, link);
				}

				return Ok(responseData);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}

		[Authorize("Customer,Seller")]
		[HttpPost("All")]
		public IActionResult GetOrders([FromBody] GetAllOrderRequestDTO request)
		{
			if (!ModelState.IsValid)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_FAILD, "INVALID", false, new()));
			}

			var accessToken = Util.GetAccessToken(HttpContext);
			var userIdFromAccessToken = jwtTokenService.GetUserIdByAccessToken(accessToken);
			if (request.UserId != userIdFromAccessToken)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_UN_AUTHORIZE, "ACCESS DENIED", false, new()));
			}

			List<Order> orders = orderRepository.GetAllOrderByUser(request.UserId, request.StatusId, request.Limit, request.Offset);
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
		[Authorize("Customer,Seller")]
		[HttpPost("Edit/Status")]
		public async Task<IActionResult> UpdateStatusOrder([FromBody] EditStatusOrderRequestDTO request)
		{
			if (!ModelState.IsValid)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_FAILD, "INVALID", false, new()));
			}
			var accessToken = Util.GetAccessToken(HttpContext);
			var userIdFromAccessToken = jwtTokenService.GetUserIdByAccessToken(accessToken);
			if (request.UserId != userIdFromAccessToken)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_UN_AUTHORIZE, "ACCESS DENIED", false, new()));
			}
			Order? order = orderRepository.GetOrderCustomer(request.OrderId);
			if (order == null)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_FAILD, "INVALID", false, new()));
			}
			else if (order.ShopId != request.ShopId)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_FAILD, "INVALID", false, new()));
			}
			else if (order.UserId != request.UserId)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_FAILD, "INVALID", false, new()));
			}
			else if (order.OrderStatusId == Constants.ORDER_CONFIRMED)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_FAILD, "INVALID", false, new()));
			}
			else if (request.StatusId != Constants.ORDER_COMPLAINT && request.StatusId != Constants.ORDER_CONFIRMED)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_FAILD, "INVALID", false, new()));
			}
			try
			{
				orderRepository.UpdateOrderStatusCustomer(request.OrderId, request.ShopId, request.StatusId);

				string title = $"{(request.StatusId == Constants.ORDER_CONFIRMED ? "Xác nhận đơn hàng thành công." : "Đơn hàng đang được khiếu nại.")}";
				string content = $"Mã đơn số {request.OrderId} {(request.StatusId == Constants.ORDER_CONFIRMED ? "đã được xác nhận." : "đang khiếu nại.")}";
				string link = Constants.FRONT_END_HISTORY_ORDER_URL;
				await hubService.SendNotification(request.UserId, title, content, link);
			}
			catch (Exception e)
			{
				return Conflict(e.Message);
			}
			return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "SUCCESS", true, new()));
		}
		[Authorize("Customer,Seller")]
		[HttpGet("User/{userId}/{orderId}")]
		public IActionResult GetOrderDetailCustomer(long userId, long orderId)
		{
			var accessToken = Util.GetAccessToken(HttpContext);
			var userIdFromAccessToken = jwtTokenService.GetUserIdByAccessToken(accessToken);
			if (userId != userIdFromAccessToken)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_UN_AUTHORIZE, "ACCESS DENIED", false, new()));
			}
			Order? order = orderRepository.GetOrderCustomer(orderId);
			if (order == null)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_FAILD, "INVALID", false, new()));
			}

			else if (order.UserId != userId)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_FAILD, "INVALID", false, new()));
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
	}

}
