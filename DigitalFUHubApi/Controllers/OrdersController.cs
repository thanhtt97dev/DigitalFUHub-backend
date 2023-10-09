using AutoMapper;
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

		[HttpPost("AddOrder")]
		//[Authorize]
		public IActionResult AddOrder([FromBody] List<AddOrderRequestDTO> addOrderRequest)
		{
			try
			{
				if (addOrderRequest == null) return BadRequest();

				(string responseCode, string message) = orderRepository.AddOrder(addOrderRequest);
				List<Order> orders = mapper.Map<List<Order>>(addOrderRequest);


				ResponseData responseData = new ResponseData();
				Status status = new Status()
				{
					Message = message,
					Ok = responseCode == Constants.RESPONSE_CODE_SUCCESS,
					ResponseCode = responseCode
				};
				responseData.Status = status;
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}

		//[Authorize]
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
					ShopId = x.ProductVariant.Product.ShopId,
					ShopName = x.ProductVariant.Product.Shop.ShopName,
					Quantity = x.Quantity,
					Price = x.Price,
					Discount = x.Discount,
					IsFeedback = x.FeedbackId == 0 ? false : true,
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
	}
}
