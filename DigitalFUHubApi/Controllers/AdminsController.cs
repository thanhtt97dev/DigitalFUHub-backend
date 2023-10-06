using AutoMapper;
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

		public AdminsController(IMapper mapper, IOrderRepository orderRepository)
		{
			this.mapper = mapper;
			this.orderRepository = orderRepository;
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

	}
}
