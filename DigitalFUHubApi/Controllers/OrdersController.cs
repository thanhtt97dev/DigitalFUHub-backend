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

		public OrdersController(IOrderRepository orderRepository,
			IReportRepository reportRepository,
			IShopRepository shopRepository,
			JwtTokenService jwtTokenService,
			IMapper mapper,
			HubService hubService)
		{
			_orderRepository = orderRepository;
			_reportRepository = reportRepository;
			_shopRepository = shopRepository;
			_jwtTokenService = jwtTokenService;
			_mapper = mapper;
			_hubService = hubService;
		}


		#region Add order (customer)
		[Authorize("Customer,Seller")]
		[HttpPost("Customer/AddOrder")]
		public async Task<IActionResult> AddOrder(AddOrderRequestDTO request)
		{
			try
			{
				ResponseData responseData = new ResponseData();
				if (request.UserId != _jwtTokenService.GetUserIdByAccessToken(User))
				{
					return Unauthorized();
				}
				if (!ModelState.IsValid)
				{
					return BadRequest();
				}
				(string responseCode, string message, int numberQuantityAvailable, Order orderInfo) =
					_orderRepository.AddOrder(request.UserId, request.ShopProducts, request.IsUseCoin);

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
					var link = Constants.FRONT_END_HISTORY_ORDER_URL + orderInfo.OrderId;
					await _hubService.SendNotification(request.UserId, title, content, link);
				}

				return Ok(responseData);
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
				if (request.UserId != _jwtTokenService.GetUserIdByAccessToken(User))
				{
					return Unauthorized();
				}
				if (!ModelState.IsValid)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_FAILD, "Invalid data", false, new()));
				}
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
		public async Task<IActionResult> UpdateStatusOrder([FromBody] EditStatusOrderRequestDTO request)
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

				string title = $"{(request.StatusId == Constants.ORDER_STATUS_CONFIRMED ? "Xác nhận đơn hàng thành công." : "Đơn hàng đang được khiếu nại.")}";
				string content = $"Mã đơn số {request.OrderId} {(request.StatusId == Constants.ORDER_STATUS_CONFIRMED ? "đã được xác nhận." : "đang khiếu nại.")}";
				string link = Constants.FRONT_END_HISTORY_ORDER_URL + request.OrderId;
				await _hubService.SendNotification(request.UserId, title, content, link);
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
						FeebackRate = od?.Feedback?.Rate ?? 0
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
				CustomerUsername = order.User.Username,
				CustomerAvatar = order.User.Avatar,
				StatusId = order.OrderStatusId,
				TotalAmount = order.TotalAmount,
				//TotalCoinDiscount = order.TotalCoinDiscount,
				//TotalPayment = order.TotalPayment,
				CouponCode = order.OrderCoupons.FirstOrDefault()?.Coupon?.CouponCode??"",
				TotalCouponDiscount = order.TotalCouponDiscount,
				BussinessFee = (order.TotalAmount - order.TotalCouponDiscount) * order.BusinessFee.Fee / 100,
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
				if(shop == null) { 
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid shop", false, new()));
				}
				List<Order> orders = _orderRepository.GetListOrderSeller(_jwtTokenService.GetUserIdByAccessToken(User),
					request.OrderId, request.Username.Trim(), fromDate, toDate, request.Status);
				if (orders.Count <= 0)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "No data", false, new()));
				}
				DateTime now = DateTime.Now;
				string reportname =  string.Format("{0}{1}{2}{3}{4}{5}{6}.xlsx", now.Year, now.Month, now.Day, now.Millisecond, now.Second, now.Minute, now.Hour);
				var exportBytes = await _reportRepository
					.ExportToExcel<SellerReportOrderResponseDTO>(_mapper.Map<List<SellerReportOrderResponseDTO>>(orders), 
					reportname, fromDate, toDate, shop.ShopName);
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
		public IActionResult GetOrders(OrdersRequestDTO requestDTO)
		{
			if (!ModelState.IsValid) return BadRequest();

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

				var orders = _orderRepository.GetOrders(orderId, requestDTO.CustomerEmail, requestDTO.ShopName, fromDate, toDate, requestDTO.Status);
				var result = _mapper.Map<List<OrdersResponseDTO>>(orders);

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

		#region Get order detail (admin)
		[Authorize("Admin")]
		[HttpPost("Admin/GetOrder/{id}")]
		public IActionResult GetOrder(int id)
		{
			if (!ModelState.IsValid) return BadRequest();

			ResponseData responseData = new ResponseData();
			Status status = new Status();

			try
			{
				var order = _orderRepository.GetOrderInfoAdmin(id);
				if (order == null)
				{
					status.Message = "Order not existed!";
					status.Ok = false;
					status.ResponseCode = Constants.RESPONSE_CODE_DATA_NOT_FOUND;
					responseData.Status = status;
					return Ok(responseData);
				}

				var result = _mapper.Map<OrderInfoResponseDTO>(order);

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

		#region Update order status (admin)
		[Authorize("Admin")]
		[HttpPost("Admin/UpdateOrderStatus")]
		public IActionResult UpdateOrderStatus(UpdateOrderStatusRequestDTO request)
		{
			if (request.OrderId == 0 || request.Status == 0) return BadRequest();
			ResponseData responseData = new ResponseData();
			Status status = new Status();

			try
			{
				int[] statusAccepted = { Constants.ORDER_STATUS_DISPUTE, Constants.ORDER_STATUS_REJECT_COMPLAINT, Constants.ORDER_STATUS_SELLER_VIOLATES };
				if (!statusAccepted.Contains(request.Status))
				{
					status.Message = "Invalid order status!";
					status.Ok = false;
					status.ResponseCode = Constants.RESPONSE_CODE_DATA_NOT_FOUND;
					responseData.Status = status;
					return Ok(responseData);
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
				}
				else if (request.Status == Constants.ORDER_STATUS_SELLER_VIOLATES)
				{
					_orderRepository.UpdateOrderStatusSellerViolates(request.OrderId, request.Note);
				}
				else
				{
					status.Message = "Invalid order status!";
					status.Ok = false;
					status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
					responseData.Status = status;
					return Ok(responseData);
				}

				// check seller have VIOLATE 

				status.Message = "Success!";
				status.Ok = true;
				status.ResponseCode = Constants.RESPONSE_CODE_SUCCESS;
				responseData.Status = status;
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
