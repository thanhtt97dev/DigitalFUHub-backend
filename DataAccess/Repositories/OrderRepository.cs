using BusinessObject.Entities;
using Comons;
using DataAccess.DAOs;
using DataAccess.IRepositories;
using DTOs.Order;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
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

		public List<Order> GetOrders(long orderId, string customerEmail, string shopName, DateTime fromDate, DateTime toDate, int status) => OrderDAO.Instance.GetOrders(orderId, customerEmail, shopName, fromDate, toDate, status);

		public (string, string, int, Order) AddOrder(long userId, List<ShopProductRequestAddOrderDTO> orders, bool isUseCoin) => OrderDAO.Instance.AddOrder(userId, orders, isUseCoin);

		public Order? GetOrder(long orderId) => OrderDAO.Instance.GetOrder(orderId);

		public Order? GetOrderDetailSeller(long userId, long orderId) => OrderDAO.Instance.GetOrderDetailSeller(userId, orderId);

		public void UpdateOrderStatusAdmin(long orderId, int status, string? note) => OrderDAO.Instance.UpdateOrderStatusAdmin(orderId, status, note);

		public void UpdateOrderStatusSellerViolates(long orderId, string? note) => OrderDAO.Instance.UpdateOrderStatusSellerViolates(orderId, note);

		public void UpdateOrderStatusRejectComplaint(long orderId, string? note) => OrderDAO.Instance.UpdateOrderStatusRejectComplaint(orderId, note);

		public Order? GetOrderForCheckingExisted(long orderId) => OrderDAO.Instance.GetOrderForCheckingExisted(orderId);

		public List<Order> GetAllOrderByUser(long userId, List<long> statusId, int limit, int offset) => OrderDAO.Instance.GetAllOrderByUser(userId, statusId, limit, offset);

		public void UpdateOrderStatusCustomer(long orderId, long shopId, int status) => OrderDAO.Instance.UpdateOrderStatusCustomer(orderId, shopId, status);

		public Order? GetOrderCustomer(long orderId, long customerId, long shopId) => OrderDAO.Instance.GetOrderCustomer(orderId, customerId, shopId);

		public Order? GetOrderCustomer(long orderId, long customerId) => OrderDAO.Instance.GetOrderCustomer(orderId, customerId);

		public List<Order> GetListOrderSeller(long userId, long orderId, string username, DateTime? fromDate, DateTime? toDate, int status)
		=> OrderDAO.Instance.GetListOrderSeller(userId, orderId, username, fromDate, toDate, status);

		public void UpdateStatusOrderDispute(long sellerId, long customerId, long orderId)
		=> OrderDAO.Instance.UpdateStatusOrderDispute(sellerId, customerId, orderId);

		public void UpdateStatusOrderRefund(long sellerId, long orderId, string note)
		=> OrderDAO.Instance.UpdateStatusOrderRefund(sellerId, orderId, note);
	}
}
