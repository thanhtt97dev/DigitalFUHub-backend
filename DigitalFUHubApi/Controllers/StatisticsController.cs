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
		private readonly JwtTokenService _jwtTokenService;

		public StatisticsController(IOrderRepository orderRepository, JwtTokenService jwtTokenService)
		{
			_orderRepository = orderRepository;
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
				})) ;
			}
			catch (Exception)
			{
				return BadRequest();
			}
		}
		#endregion
	}
}

