﻿using BusinessObject.Entities;
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
		void UpdateStatusOrderFromWaitConfirmationToConfirmInPreviousDays(int days);
		void UpdateStatusOrderFromComplaintToSellerRefundedInPreviousDays(int days);
		List<Order> GetOrders(long orderId, string customerEmail, string shopName, DateTime fromDate, DateTime toDate, int status);
		(string, string, int, Order) AddOrder(long userId, List<ShopProductRequestAddOrderDTO> orders, bool isUseCoin);
		Order? GetOrder(long orderId);
		Order? GetOrderForCheckingExisted(long orderId);
		Order? GetOrderDetailSeller(long userId, long orderId1);
		void UpdateOrderStatusSellerViolates(long orderId, string? note);
		void UpdateOrderStatusRejectComplaint(long orderId, string? note);
		void UpdateOrderStatusAdmin(long orderId, int status, string? note);
		void UpdateOrderStatusCustomer(long orderId,long shopId, int status);
		List<Order> GetAllOrderByUser(long userId,List<long> statusId, int limit, int offset);
		Order? GetOrderCustomer(long orderId, long userId, long shopId);
		Order? GetOrderCustomer(long orderId, long customerId);
		List<Order> GetListOrderSeller(long userId, string orderId, string username, DateTime? fromDate, DateTime? toDate, int status);
		void UpdateStatusOrderDispute(long sellerId, long customerId, long orderId);
		void UpdateStatusOrderRefund(long sellerId, long orderId, string note);
	}
}
