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
		List<Order> GetOrders(long orderId, string customerEmail, string shopName, DateTime fromDate, DateTime toDate, int status);
		void ConfirmOrdersWithWaitToConfirmStatus(List<Order> orders);
		void AddOrder(List<Order> orders);
		Order? GetOrder(long orderId);

    }
}
