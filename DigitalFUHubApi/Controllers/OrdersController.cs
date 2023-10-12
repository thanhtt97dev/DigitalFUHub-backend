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
		public IActionResult AddOrder([FromBody] List<AddOrderRequestDTO> request)
		{
			try
			{
				if (request == null || request.Count == 0) return BadRequest();

				ResponseData responseData = new ResponseData();
				var accessToken = Util.GetAccessToken(HttpContext);
				var userIdFromAccessToken = jwtTokenService.GetUserIdByAccessToken(accessToken);
				if(request.Count(x => x.UserId == userIdFromAccessToken) != request.Count) 
				{
					responseData.Status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
					responseData.Status.Ok = false;
					responseData.Status.Message = "Invalid params!";
					return Ok(responseData);
				}
				if (request.ElementAt(0).UserId != userIdFromAccessToken)
				{
					responseData.Status.ResponseCode = Constants.RESPONSE_CODE_UN_AUTHORIZE;
					responseData.Status.Ok = false;
					responseData.Status.Message = "Not have permission!";
					return Ok(responseData);
				}

				(string responseCode, string message) = orderRepository.AddOrder(request);

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
			ResponseData response = new ResponseData();
			if(!ModelState.IsValid)
			{
				response.Status.ResponseCode = Constants.RESPONSE_CODE_FAILD;
				response.Status.Ok = false;
				response.Status.Message = "Invalid";
				return Ok(response);
			}

			var accessToken = Util.GetAccessToken(HttpContext);
			var userIdFromAccessToken = jwtTokenService.GetUserIdByAccessToken(accessToken);
			if(request.UserId != userIdFromAccessToken) 
			{
				response.Status.ResponseCode = Constants.RESPONSE_CODE_UN_AUTHORIZE;
				response.Status.Ok = false;
				response.Status.Message = "Not have permission to get data!";
				return Ok(response);
			}

			List<Order> orders = orderRepository.GetAllOrderByUser(request.UserId, request.StatusId, request.Limit, request.Offset);
			OrderResponseDTO orderResponse = new OrderResponseDTO()
			{
				NextOffset = orders.Count < request.Limit ? -1 : request.Offset + orders.Count,
				Orders = orders.Select(x => new OrderProductResponseDTO
				{
					OrderId = x.OrderId,
					ShopId = x.ProductVariant.Product.ShopId,
					ShopName = x.ProductVariant.Product.Shop.ShopName,
					Quantity = x.Quantity,
					Price = x.Price,
					Discount = x.Discount,
					IsFeedback = x.FeedbackId == null ? false : true,
					ProductName = x.ProductVariant?.Product?.ProductName ?? "",
					ProductId = x.ProductVariant?.ProductId ?? 0,
					CouponDiscount = x.TotalCouponDiscount,
					ProductVariantName = x.ProductVariant?.Name ?? "",
					StatusId = x.OrderStatusId,
					Thumbnail = x.ProductVariant?.Product.Thumbnail ?? "",
					Assest = x.AssetInformations.Select(x => x.Asset ?? "").ToList(),
				}).ToList()
			};
			response.Status.ResponseCode = Constants.RESPONSE_CODE_SUCCESS;
			response.Status.Ok = true;
			response.Status.Message = "Success";
			response.Result = orderResponse;
			return Ok(response);
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
			} else if(order.ProductVariant.Product.Shop.UserId != request.ShopId)
			{
				response.Status.ResponseCode = Constants.RESPONSE_CODE_FAILD;
				response.Status.Ok = false;
				response.Status.Message = "Invalid";
				return Ok(response);
			} else if(order.UserId != request.UserId)
			{
				response.Status.ResponseCode = Constants.RESPONSE_CODE_FAILD;
				response.Status.Ok = false;
				response.Status.Message = "Invalid";
				return Ok(response);
			} else if(order.OrderStatusId == Constants.ORDER_CONFIRMED)
			{
				response.Status.ResponseCode = Constants.RESPONSE_CODE_FAILD;
				response.Status.Ok = false;
				response.Status.Message = "Invalid";
				return Ok(response);
			}
			else if(request.StatusId != Constants.ORDER_COMPLAINT && request.StatusId != Constants.ORDER_CONFIRMED)
			{
				response.Status.ResponseCode = Constants.RESPONSE_CODE_FAILD;
				response.Status.Ok = false;
				response.Status.Message = "Invalid";
				return Ok(response);
			}
			try
			{
				orderRepository.UpdateOrderStatusCustomer(request.OrderId,request.ShopId, request.StatusId);
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
