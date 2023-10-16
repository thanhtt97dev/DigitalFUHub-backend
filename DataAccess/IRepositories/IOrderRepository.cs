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
		void UpdateStatusOrderToConfirm();
		void UpdateStatusOrderToSellerRefunded();
		List<Order> GetOrders(long orderId, string customerEmail, string shopName, DateTime fromDate, DateTime toDate, int status);
		(string, string) AddOrder(long userId, List<ShopProductRequestAddOrderDTO> orders, bool isUseCoin);
		Order? GetOrder(long orderId);
		Order? GetOrderForCheckingExisted(long orderId);
		Order? GetSellerOrderDetail(long orderId);
		void UpdateOrderStatusSellerViolates(long orderId, string? note);
		void UpdateOrderStatusRejectComplaint(long orderId, string? note);
		void UpdateOrderStatusAdmin(long orderId, int status, string? note);
		void UpdateOrderStatusCustomer(long orderId,long shopId, int status);
		List<Order> GetAllOrderByUser(long userId,List<long> statusId, int limit, int offset);
		List<OrderCoupon> GetCouponsInOrder(long orderId);
	}
}
