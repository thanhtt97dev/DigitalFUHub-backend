using BusinessObject.Entities;
using Comons;
using DataAccess.IRepositories;
using DataAccess.Repositories;
using DigitalFUHubApi.Comons;
using DigitalFUHubApi.Services;
using DTOs.Shop;
using DTOs.Statistic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml.Style;
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
		private readonly IShopRepository _shopRepository;
		private readonly IReportProductRepository _reportProductRepository;
		private readonly IBankRepository _bankRepository;
		private readonly IUserRepository _userRepository;
		private readonly IBusinessFeeRepository _businessRepository;
		private readonly ITransactionCoinRepository _transactionCoinRepository;
		private readonly JwtTokenService _jwtTokenService;

		public StatisticsController(IOrderRepository orderRepository,
			ITransactionCoinRepository transactionCoinRepository,
			IProductRepository productRepository,
			IShopRepository shopRepository,
			IBankRepository bankRepository,
			IReportProductRepository reportProductRepository,
			IUserRepository userRepository,
			IBusinessFeeRepository businessRepository,
			JwtTokenService jwtTokenService)
		{
			_orderRepository = orderRepository;
			_transactionCoinRepository = transactionCoinRepository;
			_productRepository = productRepository;
			_shopRepository = shopRepository;
			_bankRepository = bankRepository;
			_reportProductRepository = reportProductRepository;
			_userRepository = userRepository;
			_businessRepository = businessRepository;
			_jwtTokenService = jwtTokenService;
		}


		#region statistic sales by month or annual for shop (seller)
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
				if (!Constants.ORDER_STATUS.Contains(request.StatusOrder) && Constants.ORDER_ALL != request.StatusOrder)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid type order", false, new()));
				}
				List<Order> orders = _orderRepository.GetListOrderOfShop(_jwtTokenService.GetUserIdByAccessToken(User),
					request.Month, request.Year, request.StatusOrder);
				// get statisic sales by each day of month
				if (request.Month != 0)
				{
					int numberDaysOfMonth = DateTime.DaysInMonth(request.Year, request.Month);
					List<int> daysOfMonth = Enumerable.Range(1, numberDaysOfMonth).ToList();
					
					var query = from d in daysOfMonth
								join o in orders
								on new { day = d, month = request.Month } equals
								new { day = o.OrderDate.Day, month = o.OrderDate.Month } into od
								from ord in od.DefaultIfEmpty()
								group new { d, ord } by d into tb
								select new DataStatistic
								{
									Date = tb.Key,
									Revenue = tb.Sum(x => x.ord == null ? 0 : (long)(((x?.ord?.TotalAmount ?? 0) - (x?.ord?.TotalCouponDiscount ?? 0)))),
									Profit = tb.Sum(x => x.ord == null ? 0 : (long)(((x?.ord?.TotalAmount ?? 0) - (x?.ord?.TotalCouponDiscount ?? 0))
										- ((x?.ord?.TotalAmount ?? 0) * (x?.ord?.BusinessFee?.Fee ?? 0) / 100))),
									TotalOrders = (long)tb.Count(x => x.ord != null)
								};
					return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, new StatisticSalesResponseDTO
					{
						TypeStatistic = Constants.STATISTIC_BY_MONTH,
						DataStatistics = query.ToList()
					}));
				}
				// get statisic sales by each month of year
				List<int> MonthOfYear = Enumerable.Range(1, 12).ToList();
				var query2 = from d in MonthOfYear
							 join o in orders
							 on new { month = d, year = request.Year } equals
							 new { month = o.OrderDate.Month, year = o.OrderDate.Year } into od
							 from ord in od.DefaultIfEmpty()
							 group new { d, ord } by d into tb2
							 select new DataStatistic
							 {
								 Date = tb2.Key,
								 Revenue = tb2.Sum(x => x.ord == null ? 0 : (long)(((x?.ord?.TotalAmount ?? 0) - (x?.ord?.TotalCouponDiscount ?? 0)))),
								 Profit = tb2.Sum(x => x.ord == null ? 0 : (long)(((x?.ord?.TotalAmount ?? 0) - (x?.ord?.TotalCouponDiscount ?? 0))
									- ((x?.ord?.TotalAmount ?? 0) * (x?.ord?.BusinessFee?.Fee ?? 0) / 100))),
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

		#region  statistc sales current month of shop (seller)
		[Authorize("Seller")]
		[HttpGet("CurrentMonth")]
		public IActionResult StatisticSalesCurrentMonth()
		{
			try
			{
				List<Order> orders = _orderRepository.GetListOrderOfCurrentMonth(_jwtTokenService.GetUserIdByAccessToken(User));
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, new
				{
					Revenue = orders.Count == 0 ? 0 : orders.Sum(x => (long)(((x?.TotalAmount ?? 0) - (x?.TotalCouponDiscount ?? 0)))),
					Profit = orders.Count == 0 ? 0 : orders.Sum(x => (long)(((x?.TotalAmount ?? 0) - (x?.TotalCouponDiscount ?? 0))
											- ((x?.TotalAmount ?? 0) * (x?.BusinessFee?.Fee ?? 0) / 100))),
					TotalOrders = orders.Count
				}));
			}
			catch (Exception e)
			{
				return BadRequest(e.Message);
			}
		}
		#endregion

		#region statistic todo list of shop (seller)
		[Authorize("Seller")]
		[HttpGet("TodoList")]
		public IActionResult GetTodoList()
		{
			try
			{
				List<Order> orders = _orderRepository.GetListOrderByStatus(_jwtTokenService.GetUserIdByAccessToken(User));
				List<StatisticNumberOrdersOfStatusResponseDTO> ordersStatus = orders.GroupBy(x => x.OrderStatusId)
					.Select(x => new StatisticNumberOrdersOfStatusResponseDTO
					{
						OrderStatusId = x.Key,
						Count = x.LongCount()
					}).ToList();
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

		#region statistic sales by month or annual (admin)
		[Authorize("Admin")]
		[HttpPost("Admin/Sales")]
		public IActionResult StatisticSalesAllShop(StatisticSalesRequestDTO request)
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
				if (!Constants.ORDER_STATUS.Contains(request.StatusOrder) && Constants.ORDER_ALL != request.StatusOrder)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid type order", false, new()));
				}
				List<Order> orders = _orderRepository.GetListOrderAllShop(request.Month, request.Year, request.StatusOrder);
				// get statisic sales by each day of month
				if (request.Month != 0)
				{
					int numberDaysOfMonth = DateTime.DaysInMonth(request.Year, request.Month);
					List<int> daysOfMonth = Enumerable.Range(1, numberDaysOfMonth).ToList();
					var query = from d in daysOfMonth
								join o in orders
								on new { day = d, month = request.Month } equals
								new { day = o.OrderDate.Day, month = o.OrderDate.Month } into od
								from ord in od.DefaultIfEmpty()
								group new { d, ord } by d into tb
								select new DataStatistic
								{
									Date = tb.Key,
									Revenue = tb.Sum(x => x.ord == null ? 0 : (long)(((x?.ord?.TotalAmount ?? 0) - (x?.ord?.TotalCouponDiscount ?? 0)))),
									Profit = tb.Sum(x => x.ord == null ? 0 : (long)(((x?.ord?.TotalAmount ?? 0) - (x?.ord?.TotalCouponDiscount ?? 0))
										- ((x?.ord?.TotalAmount ?? 0) * (x?.ord?.BusinessFee?.Fee ?? 0) / 100))),
									TotalOrders = (long)tb.Count(x => x.ord != null)
								};
					return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, new StatisticSalesResponseDTO
					{
						TypeStatistic = Constants.STATISTIC_BY_MONTH,
						DataStatistics = query.ToList()
					}));
				}
				// get statisic sales by each month of year
				List<int> MonthOfYear = Enumerable.Range(1, 12).ToList();
				var query2 = from d in MonthOfYear
							 join o in orders
							 on new { month = d, year = request.Year } equals
							 new { month = o.OrderDate.Month, year = o.OrderDate.Year } into od
							 from ord in od.DefaultIfEmpty()
							 group new { d, ord } by d into tb2
							 select new DataStatistic
							 {
								 Date = tb2.Key,
								 Revenue = tb2.Sum(x => x.ord == null ? 0 : (long)(((x?.ord?.TotalAmount ?? 0) - (x?.ord?.TotalCouponDiscount ?? 0)))),
								 Profit = tb2.Sum(x => x.ord == null ? 0 : (long)(((x?.ord?.TotalAmount ?? 0) - (x?.ord?.TotalCouponDiscount ?? 0))
									- ((x?.ord?.TotalAmount ?? 0) * (x?.ord?.BusinessFee?.Fee ?? 0) / 100))),
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

		#region statistic deposit and withdrawn money by month or annual (admin)
		[Authorize("Admin")]
		[HttpPost("Admin/DepositAndWithdrawnMoney")]
		public IActionResult StatisticDepositAndWithdrawnMoney(StatisticDepositAndWithdrawnRequestDTO request)
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

				List<DepositTransaction> listDeposit = _bankRepository.GetListDepositMoney(request.Month, request.Year);
				List<WithdrawTransaction> listWithdrawn = _bankRepository.GetListWithdrawnMoney(request.Month, request.Year);
				// get statisic sales by each day of month
				if (request.Month != 0)
				{
					int numberDaysOfMonth = DateTime.DaysInMonth(request.Year, request.Month);
					List<int> daysOfMonth = Enumerable.Range(1, numberDaysOfMonth).ToList();
					var query = from d in daysOfMonth
								join ld in listDeposit
								on new { day = d, month = request.Month } equals
								new { day = ld.PaidDate.Value.Day, month = ld.PaidDate.Value.Month } into tb1
								from tbDeposit in tb1.DefaultIfEmpty()
								join lw in listWithdrawn
								on new { day = d, month = request.Month } equals
								new { day = lw.PaidDate.Value.Day, month = lw.PaidDate.Value.Month } into tb2
								from tbWithdrawn in tb2.DefaultIfEmpty()
								group new { d, tbDeposit, tbWithdrawn } by d into tb3

								select new DataDepositAndWithdrawnStatistic
								{
									Date = tb3.Key,
									TotalAmountDeposit = tb3.Sum(x => x.tbDeposit == null ? 0 : x.tbDeposit.Amount),
									TotalAmountWithdrawn = tb3.Sum(x => x.tbWithdrawn == null ? 0 : x.tbWithdrawn.Amount),
								};
					return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, new StatisticDepositAndWithdrawnResponseDTO
					{
						TypeStatistic = Constants.STATISTIC_BY_MONTH,
						DataStatistics = query.ToList()
					}));
				}
				// get statisic sales by each month of year
				List<int> MonthOfYear = Enumerable.Range(1, 12).ToList();
				var query2 = from d in MonthOfYear
							 join ld in listDeposit
							 on new { month = d, year = request.Year } equals
							 new { month = ld.PaidDate.Value.Month, year = ld.PaidDate.Value.Year } into tb1
							 from tbDeposit in tb1.DefaultIfEmpty()
							 join lw in listWithdrawn
							 on new { month = d, year = request.Year } equals
							 new { month = lw.PaidDate.Value.Month, year = lw.PaidDate.Value.Year } into tb2
							 from tbWithdrawn in tb2.DefaultIfEmpty()
							 group new { d, tbDeposit, tbWithdrawn } by d into tb3
							 select new DataDepositAndWithdrawnStatistic
							 {
								 Date = tb3.Key,
								 TotalAmountDeposit = tb3.Sum(x => x.tbDeposit == null ? 0 : x.tbDeposit.Amount),
								 TotalAmountWithdrawn = tb3.Sum(x => x.tbWithdrawn == null ? 0 : x.tbWithdrawn.Amount),
							 };

				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, new StatisticDepositAndWithdrawnResponseDTO
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

		#region statistic todo list (admin)
		[Authorize("Admin")]
		[HttpGet("Admin/TodoList")]
		public IActionResult GetTodoListAdmin()
		{
			try
			{
				long numberOrdersDispute = _orderRepository.GetNumberOrdersDispute();
				long numberRequestWithdrawnMoney = _bankRepository.GetNumberRequestWithdrawnMoney();
				long numberUnprocessedReportProducts = _reportProductRepository.GetNumberUnprocessedReportProducts();
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, new
				{
					NumberOrdersDispute = numberOrdersDispute,
					NumberRequestWithdrawnMoney = numberRequestWithdrawnMoney,
					NumberUnprocessedReportProducts = numberUnprocessedReportProducts,
				}));
			}
			catch (Exception e)
			{
				return BadRequest(e.Message);
			}

		}
		#endregion

		#region  statistc sales current month all shop (admin)
		[Authorize("Admin")]
		[HttpGet("Admin/CurrentMonth")]
		public IActionResult StatisticSalesCurrentMonthAllShop()
		{
			try
			{
				long[] lsStatus = { Constants.ORDER_STATUS_CONFIRMED, Constants.ORDER_STATUS_REJECT_COMPLAINT };
				List<Order> orders = _orderRepository.GetListOrderOfCurrentMonthAllShop();
				List<Order> ordersCompleted = orders.Where(x => lsStatus.Contains(x.OrderStatusId)).ToList();
				long numberNewUserInCurrentMonth = _userRepository.GetNumberNewUserInCurrentMonth();
				long totalCoinUsedOrders = _transactionCoinRepository.GetNumberCoinUsedOrdersCurrentMonth();
				long totalCoinReceive = _transactionCoinRepository.GetNumberCoinReceiveCurrentMonth();
				long businessFeeCurrent = _businessRepository.GetBusinessFeeCurrent();
				long revenueAllShop = ordersCompleted.Count == 0 ? 0 : ordersCompleted.Sum(x => (long)(((x?.TotalAmount ?? 0) - (x?.TotalCouponDiscount ?? 0))));
				long profitAllShop = ordersCompleted.Count == 0 ? 0 : ordersCompleted.Sum(x => (long)(((x?.TotalAmount ?? 0) - (x?.TotalCouponDiscount ?? 0))
											- ((x?.TotalAmount ?? 0) * (x?.BusinessFee?.Fee ?? 0) / 100)));
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, new
				{
					RevenueAllShop = revenueAllShop,
					ProfitAllShop = profitAllShop,
					TotalOrders = orders.LongCount(),
					TotalCoinUsedOrders = totalCoinUsedOrders,
					TotalCoinReceive = totalCoinReceive,
					ProfitAdmin = revenueAllShop - profitAllShop,
					BusinessFee = businessFeeCurrent,
					NumberNewUserInCurrentMonth = numberNewUserInCurrentMonth,
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

