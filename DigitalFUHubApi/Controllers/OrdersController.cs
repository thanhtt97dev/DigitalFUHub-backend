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
using OfficeOpenXml.Style;
using System.Globalization;

namespace DigitalFUHubApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class OrdersController : ControllerBase
	{
		private readonly IOrderRepository _orderRepository;
		private readonly IReportRepository _reportRepository;
		private readonly IShopRepository _shopRepository;
		private readonly JwtTokenService _jwtTokenService;
		private readonly IMapper _mapper;
		private readonly HubService _hubService;
		private readonly MailService _mailService;

		public OrdersController(IOrderRepository orderRepository,
			IReportRepository reportRepository,
			IShopRepository shopRepository,
			JwtTokenService jwtTokenService,
			IMapper mapper,
			HubService hubService,
			MailService mailService)
		{
			_orderRepository = orderRepository;
			_reportRepository = reportRepository;
			_shopRepository = shopRepository;
			_jwtTokenService = jwtTokenService;
			_mapper = mapper;
			_hubService = hubService;
			_mailService = mailService;
		}


		#region Add order (customer)
		[Authorize("Customer,Seller")]
		[HttpPost("Customer/AddOrder")]
		public async Task<IActionResult> AddOrder(AddOrderRequestDTO request)
		{
			try
			{
				if (!ModelState.IsValid) return BadRequest();

				(string responseCode, string message, int numberQuantityAvailable, Order orderInfo) =
					_orderRepository.AddOrder(request.UserId, request.ShopProducts, request.IsUseCoin);

				if (responseCode == Constants.RESPONSE_CODE_SUCCESS)
				{
					await _hubService.SendNotification(
						orderInfo.ShopId, 
						$"Đơn hàng mới", 
						$"Mã đơn hàng #{orderInfo.OrderId}", 
						Constants.FRONT_END_SELLER_ORDER_DETAIL_URL + orderInfo.OrderId);
				}

				return Ok(new ResponseData(responseCode, message, responseCode == Constants.RESPONSE_CODE_SUCCESS, numberQuantityAvailable));
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Get all order (customer)
		[Authorize("Customer,Seller")]
		[HttpPost("Customer/List")]
		public IActionResult GetListOrders([FromBody] GetAllOrderRequestDTO request)
		{

			try
			{
				if (request.UserId != _jwtTokenService.GetUserIdByAccessToken(User)) return Unauthorized();

				if (!ModelState.IsValid) return Ok(new ResponseData(Constants.RESPONSE_CODE_FAILD, "Invalid data", false, new()));

				List<Order> orders = _orderRepository.GetAllOrderByUser(request.UserId, request.StatusId, request.Limit, request.Offset);
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
						ConversationId = x.ConversationId,
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
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, orderResponse));
			}
			catch (Exception e)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
			}

		}
		#endregion

		#region Update status order (customer)
		[Authorize("Customer,Seller")]
		[HttpPost("Customer/Edit/Status")]
		public IActionResult UpdateStatusOrder([FromBody] EditStatusOrderRequestDTO request)
		{
			try
			{
				if (request.UserId != _jwtTokenService.GetUserIdByAccessToken(User))
				{
					return Unauthorized();
				}
				if (!ModelState.IsValid)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_FAILD, "Invalid data", false, new()));
				}

				if (request.StatusId != Constants.ORDER_STATUS_COMPLAINT && request.StatusId != Constants.ORDER_STATUS_CONFIRMED)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_FAILD, "Invalid status order", false, new()));
				}

				Order? order = _orderRepository.GetOrderCustomer(request.OrderId, request.UserId, request.ShopId);
				if (order == null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Not found", false, new()));
				}

				//check vaild order status
				if (request.StatusId == Constants.ORDER_STATUS_CONFIRMED)
				{
					if (order.OrderStatusId != Constants.ORDER_STATUS_WAIT_CONFIRMATION &&
						order.OrderStatusId != Constants.ORDER_STATUS_COMPLAINT &&
						order.OrderStatusId != Constants.ORDER_STATUS_DISPUTE)
					{
						return Ok(new ResponseData(Constants.RESPONSE_CODE_ORDER_STATUS_CHANGED_BEFORE, "Order's status has been changed before!", false, new()));
					}
				}
				if (request.StatusId == Constants.ORDER_STATUS_COMPLAINT)
				{
					if (order.OrderStatusId != Constants.ORDER_STATUS_WAIT_CONFIRMATION)
					{
						return Ok(new ResponseData(Constants.RESPONSE_CODE_ORDER_STATUS_CHANGED_BEFORE, "Order's status has been changed before!", false, new()));
					}
				}

				_orderRepository.UpdateOrderStatusCustomer(request.OrderId, request.ShopId, request.StatusId, request.Note);
			}
			catch (Exception e)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
			}
			return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, new()));
		}
		#endregion

		#region Get order detail (customer)
		[Authorize("Customer,Seller")]
		[HttpGet("Customer/{userId}/{orderId}")]
		public IActionResult GetOrderDetailCustomer(long userId, long orderId)
		{
			try
			{
				if (userId != _jwtTokenService.GetUserIdByAccessToken(User))
				{
					return Unauthorized();
				}

				Order? order = _orderRepository.GetOrderCustomer(orderId, userId);
				if (order == null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Not found", false, new()));

				}
#pragma warning disable CS8604 // Possible null reference argument.
				OrderProductResponseDTO responseData = new OrderProductResponseDTO
				{
					OrderId = order.OrderId,
					Note = order.Note ?? "",
					OrderDate = order.OrderDate,
					ShopId = order.ShopId,
					ShopName = order.Shop.ShopName,
					ConversationId = order.ConversationId ?? 0,
					StatusId = order.OrderStatusId,
					TotalAmount = order.TotalAmount,
					TotalCoinDiscount = order.TotalCoinDiscount,
					TotalCouponDiscount = order.TotalCouponDiscount,
					TotalPayment = order.TotalPayment,
					HistoryOrderStatus = order.HistoryOrderStatus.Select(x => new HistoryOrderStatusResponseDTO
					{
						OrderId = x.OrderId,
						OrderStatusId = x.OrderStatusId,
						DateCreate = x.DateCreate,
						Note = x.Note
					}).OrderBy(x => x.DateCreate).ToList(),
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
						FeedbackRate = od?.Feedback?.Rate ?? 0
					}).ToList(),
				};
#pragma warning restore CS8604 // Possible null reference argument.
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, responseData));
			}
			catch (Exception e)
			{

				return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
			}

		}
		#endregion

		#region Get list orders (seller)
		[Authorize("Seller")]
		[HttpPost("Seller/List")]
		public IActionResult GetOrdersSeller(SellerOrdersRequestDTO request)
		{
			try
			{
				if (request.UserId != _jwtTokenService.GetUserIdByAccessToken(User))
				{
					return Unauthorized();
				}
				if (request == null || request.OrderId == null ||
					request.Username == null
					|| (string.IsNullOrWhiteSpace(request.FromDate) && !string.IsNullOrWhiteSpace(request.FromDate))
					|| (!string.IsNullOrWhiteSpace(request.FromDate) && string.IsNullOrWhiteSpace(request.FromDate)))
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid data", false, new()));
				}
				if (request.Page <= 0)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid page", false, new()));
				}

				int[] acceptedOrderStatus = Constants.ORDER_STATUS;
				if (!acceptedOrderStatus.Contains(request.Status) && request.Status != Constants.ORDER_ALL)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid status order", false, new()));
				}

				DateTime? fromDate;
				DateTime? toDate;
				string format = "M/d/yyyy";

				fromDate = string.IsNullOrWhiteSpace(request.FromDate) ? null : DateTime.ParseExact(request.FromDate,
					format, CultureInfo.InvariantCulture);
				toDate = string.IsNullOrWhiteSpace(request.ToDate) ? null : DateTime.ParseExact(request.ToDate,
					format, CultureInfo.InvariantCulture);
				if (fromDate > toDate && fromDate != null && toDate != null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid date", false, new()));
				}

				(long totalItems, List<Order> orders) = _orderRepository.GetListOrderSeller(request.UserId, request.OrderId,
					request.Username.Trim(), fromDate, toDate, request.Status, request.Page);

				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, new SellerListOrderResponseDTO
				{
					TotalItems = totalItems,
					Orders = _mapper.Map<List<SellerOrderResponseDTO>>(orders)
				}));
			}
			catch (Exception e)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
			}
		}
		#endregion

		#region get list order using coupon (seller)
		[Authorize("Seller")]
		[HttpGet("Seller/Coupon")]
		public IActionResult GetListOrderByCoupon(long couponId, int page)
		{
			if (page <= 0)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid page", false, new()));
			}
			(long totalItems, List<Order> orders) = _orderRepository.GetListOrderByCoupon(_jwtTokenService.GetUserIdByAccessToken(User),
				couponId, page);
			return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, new SellerListOrderCouponResponseDTO
			{
				TotalItems = totalItems,
				Orders = _mapper.Map<List<SellerOrderResponseDTO>>(orders)
			}));
		}
		#endregion

		#region Get order detail (seller)
		[Authorize("Seller")]
		[HttpGet("Seller/{userId}/{orderId}")]
		public IActionResult GetOrderDetailSeller(long userId, long orderId)
		{
			if (userId != _jwtTokenService.GetUserIdByAccessToken(User))
			{
				return Unauthorized();
			}
			Order? order = _orderRepository.GetOrderDetailSeller(userId, orderId);
			if (order == null)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Not found", false, new()));
			}

#pragma warning disable CS8604 // Possible null reference argument.
			SellerOrderDetailResponseDTO response = new SellerOrderDetailResponseDTO
			{
				OrderId = order.OrderId,
				Note = order.Note ?? "",
				OrderDate = order.OrderDate,
				ShopId = order.ShopId,
				CustomerId = order.User.UserId,
				ConversationId = order.ConversationId,
				CustomerUsername = order.User.Username,
				CustomerAvatar = order.User.Avatar,
				StatusId = order.OrderStatusId,
				TotalAmount = order.TotalAmount,
				//TotalCoinDiscount = order.TotalCoinDiscount,
				//TotalPayment = order.TotalPayment,
				CouponCode = order.OrderCoupons.FirstOrDefault()?.Coupon?.CouponCode ?? "",
				TotalCouponDiscount = order.TotalCouponDiscount,
				PercentBusinessFee = order.BusinessFee.Fee,
				BusinessFeePrice = (order.TotalAmount - order.TotalCouponDiscount) * order.BusinessFee.Fee / 100,
				AmountSellerReceive = (order.TotalAmount - order.TotalCouponDiscount) -
									((order.TotalAmount - order.TotalCouponDiscount) * order.BusinessFee.Fee / 100),
				HistoryOrderStatus = order.HistoryOrderStatus.Select(x => new SellerHistoryOrderResponseDTO
				{
					OrderId = x.OrderId,
					OrderStatusId = x.OrderStatusId,
					DateCreate = x.DateCreate,
					Note = x.Note
				}).OrderBy(x => x.DateCreate).ToList(),
				OrderDetails = order.OrderDetails.Select(od => new SellerOrderDetailProductResponseDTO
				{
					Discount = od.Discount,
					OrderDetailId = od.OrderDetailId,
					Price = od.Price,
					ProductId = od.ProductVariant.ProductId,
					ProductName = od.ProductVariant?.Product?.ProductName ?? "",
					ProductVariantId = od.ProductVariantId,
					ProductVariantName = od.ProductVariant?.Name ?? "",
					Quantity = od.Quantity,
					Thumbnail = od.ProductVariant?.Product?.Thumbnail ?? "",
					TotalAmount = od.TotalAmount,
					IsFeedback = od.IsFeedback,
					FeedbackRate = od?.Feedback?.Rate ?? 0
				}).ToList(),
			};
#pragma warning restore CS8604 // Possible null reference argument.
			return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, response));
		}
		#endregion

		#region Update status dispute (seller)
		[Authorize("Seller")]
		[HttpPost("Seller/Dispute")]
		public IActionResult UpdateDisputeOrder(SellerDisputeOrderRequestDTO request)
		{
			if (!ModelState.IsValid)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid", false, new()));
			}
			try
			{
				if (request.SellerId != _jwtTokenService.GetUserIdByAccessToken(User))
				{
					return Unauthorized();
				}
				var order = _orderRepository.GetOrder(request.OrderId);
				if (order == null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Not found", false, new()));
				}

				if (order.OrderStatusId != Constants.ORDER_STATUS_COMPLAINT)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_ORDER_STATUS_CHANGED_BEFORE, "Order's status has been changed before!", false, new()));
				}

				_orderRepository.UpdateStatusOrderDispute(request.SellerId, request.CustomerId, request.OrderId, request.Note);
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, new()));
			}
			catch (Exception e)
			{

				return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
			}
		}
		#endregion

		#region  Update status refund (seller)
		[Authorize("Seller")]
		[HttpPost("Seller/Refund")]
		public IActionResult UpdateRefundOrder(SellerRefundOrderRequestDTO request)
		{
			if (!ModelState.IsValid || string.IsNullOrWhiteSpace(request.Note))
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid data", false, new()));
			}
			try
			{
				if (request.SellerId != _jwtTokenService.GetUserIdByAccessToken(User))
				{
					return Unauthorized();
				}

				var order = _orderRepository.GetOrder(request.OrderId);
				if (order == null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Not found", false, new()));
				}

				if (order.OrderStatusId != Constants.ORDER_STATUS_COMPLAINT && order.OrderStatusId != Constants.ORDER_STATUS_DISPUTE)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_ORDER_STATUS_CHANGED_BEFORE, "Order's status has been changed before!", false, new()));
				}

				_orderRepository.UpdateStatusOrderRefund(request.SellerId, request.OrderId, request.Note.Trim());
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, new()));
			}
			catch (Exception e)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
			}
		}
		#endregion

		#region Export data orders to excel file (seller)
		[Authorize("Seller")]
		[HttpPost("Seller/Report")]
		public async Task<IActionResult> ExportOrdersToExcel(SellerExportOrdersRequestDTO request)
		{
			try
			{
				if (request == null || request.OrderId == null ||
					request.Username == null
					|| (string.IsNullOrWhiteSpace(request.FromDate) && !string.IsNullOrWhiteSpace(request.FromDate))
					|| (!string.IsNullOrWhiteSpace(request.FromDate) && string.IsNullOrWhiteSpace(request.FromDate)))
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid data", false, new()));
				}

				int[] acceptedOrderStatus = Constants.ORDER_STATUS;
				if (!acceptedOrderStatus.Contains(request.Status) && request.Status != Constants.ORDER_ALL)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid status order", false, new()));
				}

				DateTime? fromDate;
				DateTime? toDate;
				string format = "M/d/yyyy";

				fromDate = string.IsNullOrWhiteSpace(request.FromDate) ? null : DateTime.ParseExact(request.FromDate,
					format, CultureInfo.InvariantCulture);
				toDate = string.IsNullOrWhiteSpace(request.ToDate) ? null : DateTime.ParseExact(request.ToDate,
					format, CultureInfo.InvariantCulture);
				if (fromDate > toDate && fromDate != null && toDate != null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid date", false, new()));
				}
				Shop? shop = _shopRepository.GetShopById(_jwtTokenService.GetUserIdByAccessToken(User));
				if (shop == null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid shop", false, new()));
				}
				List<Order> orders = _orderRepository.GetListOrderSeller(_jwtTokenService.GetUserIdByAccessToken(User),
					request.OrderId, request.Username.Trim(), fromDate, toDate, request.Status);
				if (orders.Count <= 0)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "No data", false, new()));
				}
				DateTime now = DateTime.Now;
				string reportname = string.Format("{0}{1}{2}{3}{4}{5}{6}.xlsx", now.Year, now.Month, now.Day, now.Millisecond, now.Second, now.Minute, now.Hour);
				var exportBytes = await _reportRepository
					.ExportToExcel<SellerReportOrderResponseDTO>(_mapper.Map<List<SellerReportOrderResponseDTO>>(orders),
					reportname, fromDate, toDate, shop.ShopName, request.Status);
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true,
					File(exportBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", reportname)));
			}
			catch (Exception e)
			{
				return BadRequest(e.Message);
			}
		}
		#endregion

		#region Get list orders (admin)
		[Authorize("Admin")]
		[HttpPost("Admin/All")]
		public IActionResult GetOrders(OrdersRequestDTO request)
		{
			if (!ModelState.IsValid) return BadRequest();
			int[] acceptedOrderStatus = Constants.ORDER_STATUS;
			if (!acceptedOrderStatus.Contains(request.Status) && request.Status != Constants.ORDER_ALL)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid order status!", false, new { }));
			}

			try
			{

				(bool isValid, DateTime? fromDate, DateTime? toDate) = Util.GetFromDateToDate(request.FromDate, request.ToDate);
				if (!isValid)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid date", false, new()));
				}

				long orderId;
				long.TryParse(request.OrderId, out orderId);

				long shopId;
				long.TryParse(request.ShopId, out shopId);

				var totalRecord = _orderRepository.GetNumberOrders(orderId, request.CustomerEmail, shopId, request.ShopName, fromDate, toDate, request.Status);
				var numberPages = totalRecord / Constants.PAGE_SIZE + 1;
				if (request.Page > numberPages || request.Page == 0)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid number page", false, new()));
				}

				var orders = _orderRepository.GetOrders(orderId, request.CustomerEmail, shopId, request.ShopName, fromDate, toDate, request.Status, request.Page);

				var result = new
				{
					Total = totalRecord,
					Orders = _mapper.Map<List<OrdersResponseDTO>>(orders)
				};
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, result));
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Get order detail (admin)
		[Authorize("Admin")]
		[HttpPost("Admin/GetOrder/{id}")]
		public IActionResult GetOrder(int id)
		{
			if (!ModelState.IsValid) return BadRequest();
			try
			{
				var order = _orderRepository.GetOrderInfoAdmin(id);
				if (order == null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Order not existed!", false, new { }));
				}

				var result = _mapper.Map<OrderInfoResponseDTO>(order);

				return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Order not existed!", true, result));
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Update order status (admin)
		[Authorize("Admin")]
		[HttpPost("Admin/UpdateOrderStatus")]
		public async Task<IActionResult> UpdateOrderStatus(UpdateOrderStatusRequestDTO request)
		{
			if (request.OrderId == 0 || request.Status == 0) return BadRequest();

			try
			{
				int[] statusAccepted = { Constants.ORDER_STATUS_DISPUTE, Constants.ORDER_STATUS_REJECT_COMPLAINT, Constants.ORDER_STATUS_SELLER_VIOLATES };
				if (!statusAccepted.Contains(request.Status))
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Invalid order status", false, new { }));
				}

				if (request.Status != Constants.ORDER_STATUS_REJECT_COMPLAINT && request.Status != Constants.ORDER_STATUS_SELLER_VIOLATES)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid status", false, new()));
				}

				var order = _orderRepository.GetOrder(request.OrderId);
				if (order == null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Not found", false, new()));
				}

				if (order.OrderStatusId != Constants.ORDER_STATUS_DISPUTE)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_ORDER_STATUS_CHANGED_BEFORE, "Order's status has been changed before!", false, new()));
				}

				if (request.Status == Constants.ORDER_STATUS_REJECT_COMPLAINT)
				{
					_orderRepository.UpdateOrderStatusRejectComplaint(request.OrderId, request.Note);
					await _mailService.SendMailRejectComplantForCustomer(order);
					// send notification to customer
					await _hubService.SendNotification(order.UserId, $"Từ chối hoàn tiền", $"Mã đơn hàng #{order.OrderId}", Constants.FRONT_END_HISTORY_ORDER_URL + order.OrderId);
				}
				else if (request.Status == Constants.ORDER_STATUS_SELLER_VIOLATES)
				{
					_orderRepository.UpdateOrderStatusSellerViolates(request.OrderId, request.Note);
					var totalNumbnerOrderSellerViolates = _orderRepository.GetTotalNumberOrderSellerViolates(order.ShopId);
					//warning
					if (totalNumbnerOrderSellerViolates <= Constants.MAX_NUMBER_ORDER_CAN_VIOLATES_OF_A_SHOP)
					{
						await _mailService.SendMailWarningSellerViolate(order, totalNumbnerOrderSellerViolates);
					}
					//BAN
					else
					{
						_shopRepository.UpdateBanShop(order.ShopId);
						await _mailService.SendMailBanShop(order, totalNumbnerOrderSellerViolates);
					}
					await _mailService.SendMailOrderRefundMoneyForCustomer(order);
					// send notification to seller
					await _hubService.SendNotification(order.ShopId, $"Đơn hàng VI PHẠM", $"Mã đơn hàng #{order.OrderId}", Constants.FRONT_END_SELLER_ORDER_DETAIL_URL + order.OrderId);
					// send notification to customer
					await _hubService.SendNotification(order.UserId, $"Hoàn tiền", $"Mã đơn hàng #{order.OrderId}", Constants.FRONT_END_HISTORY_ORDER_URL + order.OrderId);
				}
				else
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid order status!", false, new { }));
				}

				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success!", true, new { }));
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Get orders for export report
		[Authorize("Admin")]
		[HttpPost("Admin/Report")]
		public IActionResult GetOrdersForReport(GetOrderForReportDTO request)
		{
			if (!ModelState.IsValid) return BadRequest();
			int[] acceptedOrderStatus = Constants.ORDER_STATUS;
			if (!acceptedOrderStatus.Contains(request.Status) && request.Status != Constants.ORDER_ALL)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid order status!", false, new { }));
			}

			try
			{

				(bool isValid, DateTime? fromDate, DateTime? toDate) = Util.GetFromDateToDate(request.FromDate, request.ToDate);
				if (!isValid)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid date", false, new()));
				}

				long orderId;
				long.TryParse(request.OrderId, out orderId);

				long shopId;
				long.TryParse(request.ShopId, out shopId);

				var orders = _orderRepository.GetOrdersForReport(orderId, request.CustomerEmail, shopId, request.ShopName, fromDate, toDate, request.Status);

				var result = _mapper.Map<List<OrdersResponseDTO>>(orders);

				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, result));
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion
	}
}
