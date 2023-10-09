using AutoMapper;
using Azure.Core;
using BusinessObject.Entities;
using Comons;
using DataAccess.IRepositories;
using DataAccess.Repositories;
using DigitalFUHubApi.Comons;
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
		private readonly IMapper _mapper;
		private readonly IOrderRepository _orderRepository;

		public OrdersController(IMapper mapper, IOrderRepository orderRepository)
		{
			_mapper = mapper;
			_orderRepository = orderRepository;
		}

		[HttpPost("AddOrder")]
		//[Authorize]
		public IActionResult AddOrder([FromBody] List<AddOrderRequestDTO> addOrderRequest)
		{
			try
			{
				if (addOrderRequest == null) return BadRequest();

				List<Order> orders = _mapper.Map<List<Order>>(addOrderRequest);
				(string responseCode, string message) = _orderRepository.AddOrder(orders);

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

		[Authorize]
		[HttpPost("All")]
		public IActionResult GetOrders([FromBody] GetAllOrderRequestDTO request)
		{
			ResponseData response = new ResponseData();
			
			return Ok(response);
		}
	}
}
