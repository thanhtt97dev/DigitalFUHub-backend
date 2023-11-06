using BusinessObject;
using BusinessObject.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DAOs
{
	internal class TransactionCoinDAO
	{
		private static TransactionCoinDAO? instance;
		private static readonly object instanceLock = new object();

		public static TransactionCoinDAO Instance
		{
			get
			{
				lock (instanceLock)
				{
					if (instance == null)
					{
						instance = new TransactionCoinDAO();
					}
				}
				return instance;
			}
		}

		internal List<TransactionCoin> GetHistoryTransactionInternal(long orderId, string email, DateTime fromDate, DateTime toDate, int transactionCoinTypeId)
		{
			List<TransactionCoin> transactions = new List<TransactionCoin>();
			using (DatabaseContext context = new DatabaseContext())
			{
				transactions = context.TransactionCoin
								.Include(x => x.User)
								.Where(x =>
									fromDate <= x.DateCreate && toDate >= x.DateCreate &&
									x.User.Email.Contains(email) &&
									(orderId == 0 ? true : x.OrderId == orderId) &&
									(transactionCoinTypeId == 0 ? true : x.TransactionCoinTypeId == transactionCoinTypeId)
									).OrderByDescending(x => x.DateCreate).ToList();
			}
			return transactions;
		}
	}
}
