using BusinessObject.Entities;
using DataAccess.DAOs;
using DataAccess.IRepositories;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
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

		public void ConfirmOrdersWithWaitToConfirmStatus(List<Order> orders) => OrderDAO.Instance.ConfirmOrdersWithWaitToConfirmStatus(orders);

	}
}
