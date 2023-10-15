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
		private readonly IMapper mapper;
		private readonly IOrderRepository orderRepository;
		private readonly JwtTokenService jwtTokenService;

		public OrdersController(IMapper mapper, IOrderRepository orderRepository, JwtTokenService jwtTokenService)
		{
			this.mapper = mapper;
			this.orderRepository = orderRepository;
			this.jwtTokenService = jwtTokenService;
		}

		[Authorize("Customer,Seller")]
		[HttpPost("AddOrder")]
		public IActionResult AddOrder(AddOrderRequestDTO request)
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

				(string responseCode, string message) = orderRepository.AddOrder(request.UserId, request.ShopProducts, request.IsUseCoin);

				responseData.Status.ResponseCode = responseCode;
				responseData.Status.Ok = responseCode == Constants.RESPONSE_CODE_SUCCESS;
				responseData.Status.Message = message;
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
		public IActionResult UpdateStatusOrder([FromBody] EditStatusOrderRequestDTO request)
		{
			ResponseData response = new ResponseData();
			if (!ModelState.IsValid)
			{
				response.Status.ResponseCode = Constants.RESPONSE_CODE_FAILD;
				response.Status.Ok = false;
				response.Status.Message = "Invalid";
			}
			Order? order = orderRepository.GetOrder(request.OrderId);
			if (order == null)
			{
				response.Status.ResponseCode = Constants.RESPONSE_CODE_DATA_NOT_FOUND;
				response.Status.Ok = false;
				response.Status.Message = "Invalid";
				return Ok(response);
			}
			else if (true)
			//} else if(order.ProductVariant.Product.Shop.UserId != request.ShopId)
			{
				response.Status.ResponseCode = Constants.RESPONSE_CODE_FAILD;
				response.Status.Ok = false;
				response.Status.Message = "Invalid";
				return Ok(response);
			}
			else if (order.UserId != request.UserId)
			{
				response.Status.ResponseCode = Constants.RESPONSE_CODE_FAILD;
				response.Status.Ok = false;
				response.Status.Message = "Invalid";
				return Ok(response);
			}
			else if (order.OrderStatusId == Constants.ORDER_CONFIRMED)
			{
				response.Status.ResponseCode = Constants.RESPONSE_CODE_FAILD;
				response.Status.Ok = false;
				response.Status.Message = "Invalid";
				return Ok(response);
			}
			else if (request.StatusId != Constants.ORDER_COMPLAINT && request.StatusId != Constants.ORDER_CONFIRMED)
			{
				response.Status.ResponseCode = Constants.RESPONSE_CODE_FAILD;
				response.Status.Ok = false;
				response.Status.Message = "Invalid";
				return Ok(response);
			}
			try
			{
				orderRepository.UpdateOrderStatusCustomer(request.OrderId, request.ShopId, request.StatusId);
			}
			catch (Exception e)
			{
				return Conflict(e.Message);
			}
			response.Status.ResponseCode = Constants.RESPONSE_CODE_SUCCESS;
			response.Status.Ok = true;
			response.Status.Message = "Success";
			return Ok(response);
		}
	}

}
