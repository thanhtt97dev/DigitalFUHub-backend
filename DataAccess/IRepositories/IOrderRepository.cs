using BusinessObject.Entities;
using DTOs.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.IRepositories
{
	public interface IOrderRepository
	{
		List<Order> GetAllOrderWaitToConfirm(int days);
		void UpdateStatusOrderToConfirm(List<Order> orders);
		List<Order> GetAllOrderComplaint(int days);
		void UpdateStatusOrderToSellerRefunded(List<Order> orders);
		List<Order> GetOrders(long orderId, string customerEmail, string shopName, DateTime fromDate, DateTime toDate, int status);
		(string, string) AddOrder(List<AddOrderRequestDTO> orders);
		Order? GetOrder(long orderId);
		Order? GetOrderForCheckingExisted(long orderId);
		Order? GetSellerOrderDetail(long orderId);
		void UpdateOrderStatusSellerViolates(long orderId, string? note);
		void UpdateOrderStatusRejectComplaint(long orderId, string? note);
		void UpdateOrderStatusAdmin(long orderId, int status, string? note);
		void UpdateOrderStatusCustomer(long orderId, int status);
		List<Order> GetAllOrderByUser(long userId,List<long> statusId, int limit, int offset);
	}
}
