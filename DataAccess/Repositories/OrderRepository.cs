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

namespace DataAccess.Repositories
{
	public class OrderRepository : IOrderRepository
	{
		public List<Order> GetAllOrderWaitToConfirm(int days) => OrderDAO.Instance.GetAllOrderWaitToConfirm(days);
		public void UpdateStatusOrderToConfirm(List<Order> orders) => OrderDAO.Instance.UpdateStatusOrderToConfirm(orders);
		public List<Order> GetAllOrderComplaint(int days) => OrderDAO.Instance.GetAllOrderComplaint(days);
		public void UpdateStatusOrderToSellerRefunded(List<Order> orders) => OrderDAO.Instance.UpdateStatusOrderToSellerRefunded(orders);

		public List<Order> GetOrders(long orderId, string customerEmail, string shopName, DateTime fromDate, DateTime toDate, int status) => OrderDAO.Instance.GetOrders(orderId, customerEmail, shopName, fromDate, toDate, status);

		public (string, string) AddOrder(List<AddOrderRequestDTO> orders) => OrderDAO.Instance.AddOrder(orders);

		public Order? GetOrder(long orderId) => OrderDAO.Instance.GetOrder(orderId);

		public Order? GetSellerOrderDetail(long orderId) => OrderDAO.Instance.GetSellerOrderDetail(orderId);
		
		public void UpdateOrderStatusAdmin(long orderId, int status, string? note) => OrderDAO.Instance.UpdateOrderStatusAdmin(orderId, status, note);
		
		public void UpdateOrderStatusSellerViolates(long orderId, string? note) => OrderDAO.Instance.UpdateOrderStatusSellerViolates(orderId, note);
		
		public void UpdateOrderStatusRejectComplaint(long orderId, string? note) => OrderDAO.Instance.UpdateOrderStatusRejectComplaint(orderId, note);

		public Order? GetOrderForCheckingExisted(long orderId) => OrderDAO.Instance.GetOrderForCheckingExisted(orderId);

		public List<Order> GetAllOrderByUser(long userId,List<long> statusId, int limit, int offset) => OrderDAO.Instance.GetAllOrderByUser(userId,statusId, limit, offset);

		public void UpdateOrderStatusCustomer(long orderId,long shopId, int status)
		{
			OrderDAO.Instance.UpdateOrderStatusCustomer(orderId,shopId, status);
		}

		public List<OrderCoupon> GetCouponsInOrder(long orderId) => OrderDAO.Instance.GetCouponsInOrder(orderId);

	}
}
