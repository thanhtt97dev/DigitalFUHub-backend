using BusinessObject.Entities;
using Comons;
using DataAccess.IRepositories;
using DigitalFUHubApi.Comons;
using DigitalFUHubApi.Services;
using DTOs.Shop;
using DTOs.Statistic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DigitalFUHubApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class StatisticsController : ControllerBase
	{
		private readonly IOrderRepository _orderRepository;
		private readonly IProductRepository _productRepository;
		private readonly JwtTokenService _jwtTokenService;

		public StatisticsController(IOrderRepository orderRepository, IProductRepository productRepository, JwtTokenService jwtTokenService)
		{
			_orderRepository = orderRepository;
			_productRepository = productRepository;
			_jwtTokenService = jwtTokenService;
		}


		#region
		[Authorize("Seller")]
		[HttpPost("Sales")]
		public IActionResult StatisticSales(StatisticSalesRequestDTO request)
		{
			try
			{
				if (request == null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid data", false, new()));
				}
				if (request.Month < 0 || request.Month > 12)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid month", false, new()));
				}
				List<Order> orders = _orderRepository.GetListOrderOfShop(_jwtTokenService.GetUserIdByAccessToken(User),
					request.Month, request.Year, request.TypeOrders);
				// get statisic sales by each day of month
				if (request.Month != 0)
				{
					int numberDaysOfMonth = DateTime.DaysInMonth(request.Year, request.Month);
					List<int> daysOfMonth = new List<int>();
					for (int i = 1; i <= numberDaysOfMonth; i++)
					{
						daysOfMonth.Add(i);
					}
					var query = from d in daysOfMonth
								join o in orders
								on new { day = d, month = request.Month } equals
								new { day = o.OrderDate.Day, month = o.OrderDate.Month } into od
								from ord in od.DefaultIfEmpty()
								group new { d, ord } by d into tb
								select new DataStatistic
								{
									Date = tb.Key,
									Revenue = tb.Sum(x => x.ord == null ? 0 : (long)(((x?.ord?.TotalAmount ?? 0) - (x?.ord?.TotalCouponDiscount ?? 0))
										- (((x?.ord?.TotalAmount ?? 0) - (x?.ord?.TotalCouponDiscount ?? 0))
										* (x?.ord?.BusinessFee?.Fee ?? 0) / 100))),
									TotalOrders = (long)tb.Count(x => x.ord != null)
								};
					return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, new StatisticSalesResponseDTO
					{
						TypeStatistic = Constants.STATISTIC_BY_MONTH,
						DataStatistics = query.ToList()
					}));
				}
				// get statisic sales by each month of year
				List<int> MonthOfYear = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
				var query2 = from d in MonthOfYear
							 join o in orders
							 on new { month = d, year = request.Year } equals
							 new { month = o.OrderDate.Month, year = o.OrderDate.Year } into od
							 from ord in od.DefaultIfEmpty()
							 group new { d, ord } by d into tb2
							 select new DataStatistic
							 {
								 Date = tb2.Key,
								 Revenue = tb2.Sum(x => x.ord == null ? 0 : (long)(((x?.ord?.TotalAmount ?? 0) - (x?.ord?.TotalCouponDiscount ?? 0))
									- (((x?.ord?.TotalAmount ?? 0) - (x?.ord?.TotalCouponDiscount ?? 0))
									* (x?.ord?.BusinessFee?.Fee ?? 0) / 100))),
								 TotalOrders = (long)tb2.Count(x => x.ord != null)
							 };

				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, new StatisticSalesResponseDTO
				{
					TypeStatistic = Constants.STATISTIC_BY_YEAR,
					DataStatistics = query2.ToList()
				}));
			}
			catch (Exception e)
			{
				return BadRequest(e.Message);
			}
		}
		#endregion

		#region 
		[Authorize("Seller")]
		[HttpGet("CurrentMonth")]
		public IActionResult StatisticSalesCurrentMonth()
		{
			try
			{
				List<Order> orders = _orderRepository.GetListOrderOfCurrentMonth(_jwtTokenService.GetUserIdByAccessToken(User));
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, new
				{
					Revenue = orders.Count == 0 ? 0 : orders.Sum(x => (long)(((x?.TotalAmount ?? 0) - (x?.TotalCouponDiscount ?? 0))
										- ((x?.TotalAmount ?? 0) - (x?.TotalCouponDiscount ?? 0)))),
					Profit = orders.Count == 0 ? 0 : orders.Sum(x => (long)(((x?.TotalAmount ?? 0) - (x?.TotalCouponDiscount ?? 0))
											- (((x?.TotalAmount ?? 0) - (x?.TotalCouponDiscount ?? 0)) * (x?.BusinessFee?.Fee ?? 0) / 100))),
					TotalOrders = orders.Count
				}));
			}
			catch (Exception e)
			{
				return BadRequest(e.Message);
			}
		}
		#endregion

		#region 
		[Authorize("Seller")]
		[HttpGet("TodoList")]
		public IActionResult GetTodoList()
		{
			try
			{
				List<StatisticNumberOrdersOfStatusResponseDTO> ordersStatus = _orderRepository.GetNumberOrderByStatus(_jwtTokenService.GetUserIdByAccessToken(User));
				long productsOutOfStock = _productRepository.GetNumberProductsOutOfStock(_jwtTokenService.GetUserIdByAccessToken(User));
				long totalOrdersWaitConfirm = ordersStatus.FirstOrDefault(x => x.OrderStatusId == Constants.ORDER_STATUS_WAIT_CONFIRMATION)?.Count ?? 0;
				long totalOrdersComplaint = ordersStatus.FirstOrDefault(x => x.OrderStatusId == Constants.ORDER_STATUS_COMPLAINT)?.Count ?? 0;
				long totalOrdersDispute = ordersStatus.FirstOrDefault(x => x.OrderStatusId == Constants.ORDER_STATUS_DISPUTE)?.Count ?? 0;
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, new
				{
					TotalOrdersWaitConfirm = totalOrdersWaitConfirm,
					TotalOrdersComplaint = totalOrdersComplaint,
					TotalOrdersDispute = totalOrdersDispute,
					TotalProductsOutOfStock = productsOutOfStock,
				}));
			}
			catch (Exception e)
			{
				return BadRequest(e.Message);
			}
			
		}
		#endregion
	}
}

