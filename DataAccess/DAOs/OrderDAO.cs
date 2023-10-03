using BusinessObject;
using BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DAOs
{
	public class OrderDAO
	{
		private static OrderDAO? instance;
		private static readonly object instanceLock = new object();

		public static OrderDAO Instance
		{
			get
			{
				lock (instanceLock)
				{
					if (instance == null)
					{
						instance = new OrderDAO();
					}
				}
				return instance;
			}
		}

		internal List<Order> GetAllOrderWaitToConfirm(int days)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var orders = context.Order.ToList();
					//.Where(x => x.OrderStatusId == Cons).ToList();
				return orders;
			}
		}
	}
}
