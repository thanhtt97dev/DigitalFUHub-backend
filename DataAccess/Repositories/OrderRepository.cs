using BusinessObject.Entities;
using Comons;
using DataAccess.DAOs;
using DataAccess.IRepositories;
using DTOs.Order;
using DTOs.Statistic;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DataAccess.Repositories
{
	public class OrderRepository : IOrderRepository
	{
		public void UpdateStatusOrderFromWaitConfirmationToConfirmInPreviousDays(int days) => OrderDAO.Instance.UpdateStatusOrderFromWaitConfirmationToConfirmInPreviousDays(days);
		public void UpdateStatusOrderFromComplaintToSellerRefundedInPreviousDays(int days) => OrderDAO.Instance.UpdateStatusOrderFromComplaintToSellerRefundedInPreviousDays(days);
		public int GetNumberOrders(long orderId, string customerEmail, long shopId, string shopName, DateTime? fromDate, DateTime? toDate, int status) => OrderDAO.Instance.GetNumberOrders(orderId, customerEmail, shopId, shopName, fromDate, toDate, status);
		public List<Order> GetOrders(long orderId, string customerEmail, long shopId, string shopName, DateTime? fromDate, DateTime? toDate, int status, int page) => OrderDAO.Instance.GetOrders(orderId, customerEmail, shopId, shopName, fromDate, toDate, status, page);
		public (string, string, int, Order) AddOrder(long userId, List<ShopProductRequestAddOrderDTO> orders, bool isUseCoin) => OrderDAO.Instance.AddOrder(userId, orders, isUseCoin);

		public Order? GetOrderInfoAdmin(long orderId) => OrderDAO.Instance.GetOrderInfoAdmin(orderId);

		public Order? GetOrderDetailSeller(long userId, long orderId) => OrderDAO.Instance.GetOrderDetailSeller(userId, orderId);

		public void UpdateOrderStatusAdmin(long orderId, int status, string? note) => OrderDAO.Instance.UpdateOrderStatusAdmin(orderId, status, note);

		public void UpdateOrderStatusSellerViolates(long orderId, string note) => OrderDAO.Instance.UpdateOrderStatusSellerViolates(orderId, note);

		public void UpdateOrderStatusRejectComplaint(long orderId, string note) => OrderDAO.Instance.UpdateOrderStatusRejectComplaint(orderId, note);

		public Order? GetOrderForCheckingExisted(long orderId) => OrderDAO.Instance.GetOrderForCheckingExisted(orderId);

		public List<Order> GetAllOrderByUser(long userId, List<long> statusId, int limit, int offset) => OrderDAO.Instance.GetAllOrderByUser(userId, statusId, limit, offset);

		public void UpdateOrderStatusCustomer(long orderId, long shopId, int status, string note) => OrderDAO.Instance.UpdateOrderStatusCustomer(orderId, shopId, status, note);

		public Order? GetOrderCustomer(long orderId, long customerId, long shopId) => OrderDAO.Instance.GetOrderCustomer(orderId, customerId, shopId);

		public Order? GetOrderCustomer(long orderId, long customerId) => OrderDAO.Instance.GetOrderCustomer(orderId, customerId);

		public (long, List<Order>) GetListOrderSeller(long userId, string orderId, string username, DateTime? fromDate,
			DateTime? toDate, int status, int page)
		=> OrderDAO.Instance.GetListOrderSeller(userId, orderId, username, fromDate, toDate, status, page);

		public void UpdateStatusOrderDispute(long sellerId, long customerId, long orderId, string note)
		=> OrderDAO.Instance.UpdateStatusOrderDispute(sellerId, customerId, orderId, note);

		public void UpdateStatusOrderRefund(long sellerId, long orderId, string note)
		=> OrderDAO.Instance.UpdateStatusOrderRefund(sellerId, orderId, note);

		public Order? GetOrder(long orderId) => OrderDAO.Instance.GetOrder(orderId);

		public OrderDetail? GetOrderDetail(long orderDetailId) => OrderDAO.Instance.GetOrderDetail(orderDetailId);

		public (long totalItem, List<Order> orders) GetListOrderByCoupon(long userId, long couponId, int page)
		=> OrderDAO.Instance.GetListOrderByCoupon(userId, couponId, page);

		public List<Order> GetListOrderSeller(long userId, string orderId, string username, DateTime? fromDate, DateTime? toDate, int status)
		=> OrderDAO.Instance.GetListOrderSeller(userId, orderId, username, fromDate, toDate, status);

		
		public List<Order> GetListOrderOfShop(long userId, int month, int year, int typeOrders)
		=> OrderDAO.Instance.GetListOrderOfShop(userId, month, year, typeOrders);

		public List<Order> GetListOrderOfCurrentMonth(long userId)
		=> OrderDAO.Instance.GetListOrderOfCurrentMonth(userId);

		public List<Order> GetListOrderByStatus(long userId)
		=> OrderDAO.Instance.GetListOrderByStatus(userId);

		public List<Order> GetOrdersForReport(long orderId, string customerEmail, long shopId, string shopName, DateTime? fromDate, DateTime? toDate, int status) => OrderDAO.Instance.GetOrdersForReport(orderId, customerEmail, shopId, shopName, fromDate, toDate, status);

		public int GetTotalNumberOrderSellerViolates(long shopId) => OrderDAO.Instance.GetTotalNumberOrderSellerViolates(shopId);

		public List<Order> GetListOrderAllShop(int month, int year, int typeOrders)
		=> OrderDAO.Instance.GetListOrderAllShop(month,  year,  typeOrders);

		public long GetNumberOrdersDispute()
		=> OrderDAO.Instance.GetNumberOrdersDispute();

		public List<Order> GetListOrderOfCurrentMonthAllShop()
		=> OrderDAO.Instance.GetListOrderOfCurrentMonthAllShop();
	}
}
