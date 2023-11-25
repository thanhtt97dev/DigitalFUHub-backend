using BusinessObject.Entities;
using DTOs.Order;
using DTOs.Statistic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.IRepositories
{
	public interface IOrderRepository
	{
		void UpdateStatusOrderFromWaitConfirmationToConfirmInPreviousDays(int days);
		void UpdateStatusOrderFromComplaintToSellerRefundedInPreviousDays(int days);
		List<Order> GetOrders(long orderId, string customerEmail, long shopId, string shopName, DateTime? fromDate, DateTime? toDate, int status, int page);
		(string, string, int, Order) AddOrder(long userId, List<ShopProductRequestAddOrderDTO> orders, bool isUseCoin);
		Order? GetOrderInfoAdmin(long orderId);
		Order? GetOrderForCheckingExisted(long orderId);
		Order? GetOrderDetailSeller(long userId, long orderId1);
		void UpdateOrderStatusSellerViolates(long orderId, string note);
		void UpdateOrderStatusRejectComplaint(long orderId, string note);
		void UpdateOrderStatusAdmin(long orderId, int status, string? note);
		void UpdateOrderStatusCustomer(long orderId,long shopId, int status, string note);
		List<Order> GetAllOrderByUser(long userId,List<long> statusId, int limit, int offset);
		Order? GetOrderCustomer(long orderId, long userId, long shopId);
		Order? GetOrderCustomer(long orderId, long customerId);
		(long, List<Order>) GetListOrderSeller(long userId, string orderId, string username, DateTime? fromDate, 
			DateTime? toDate, int status, int page);
		void UpdateStatusOrderDispute(long sellerId, long customerId, long orderId, string note);
		void UpdateStatusOrderRefund(long sellerId, long orderId, string note);
		Order? GetOrder(long orderId);
		OrderDetail? GetOrderDetail(long orderDetailId);
		(long totalItem, List<Order> orders) GetListOrderByCoupon(long userId, long couponId, int page);
		List<Order> GetListOrderSeller(long userId, string orderId, string v, DateTime? fromDate, DateTime? toDate, int status);
		int GetNumberOrders(long orderId, string customerEmail, long shopId, string shopName, DateTime? fromDate, DateTime? toDate, int status);
		List<Order> GetListOrderOfShop(long userId, int month, int year, int typeOders);
		List<Order> GetListOrderOfCurrentMonth(long userId);
		List<StatisticNumberOrdersOfStatusResponseDTO> GetNumberOrderByStatus(long userId);
	}
}
