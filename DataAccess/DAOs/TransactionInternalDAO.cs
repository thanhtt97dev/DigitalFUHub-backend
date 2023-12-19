using BusinessObject;
using BusinessObject.Entities;
using Comons;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DAOs
{
	internal class TransactionInternalDAO
	{
		private static TransactionInternalDAO? instance;
		private static readonly object instanceLock = new object();

		public static TransactionInternalDAO Instance
		{
			get
			{
				lock (instanceLock)
				{
					if (instance == null)
					{
						instance = new TransactionInternalDAO();
					}
				}
				return instance;
			}
		}

		internal void AddTransactionsForOrderConfirmed(Order order)
		{
			throw new NotImplementedException();
		}

		internal List<TransactionInternal> GetHistoryTransactionInternal(long orderId, string email, DateTime? fromDate, DateTime? toDate, int transactionTypeId, int page)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var transactions = context.TransactionInternal
								.Include(x => x.User)
								.Where
								(x =>
									((fromDate != null && toDate != null) ? fromDate <= x.DateCreate && toDate >= x.DateCreate : true) &&
									x.User.Email.Contains(email) &&
									(orderId == 0 ? true : x.OrderId == orderId) &&
									(transactionTypeId == 0 ? true : x.TransactionInternalTypeId == transactionTypeId)
								)
								.OrderByDescending(x => x.DateCreate)
								.Skip((page - 1) * Constants.PAGE_SIZE)
								.Take(Constants.PAGE_SIZE)
								.ToList();
				return transactions;

			}
		}

		internal int GetNumberTransactionInternal(long orderId, string email, DateTime? fromDate, DateTime? toDate, int transactionTypeId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var transactions = context.TransactionInternal
								.Include(x => x.User)
								.Where
								(x =>
									((fromDate != null && toDate != null) ? fromDate <= x.DateCreate && toDate >= x.DateCreate : true) &&
									x.User.Email.Contains(email) &&
									(orderId == 0 ? true : x.OrderId == orderId) &&
									(transactionTypeId == 0 ? true : x.TransactionInternalTypeId == transactionTypeId)
								)
								.Count();
				return transactions;
			}
		}

		internal List<TransactionInternal> GetDataReportTransactionInternal(long orderId, string email, DateTime? fromDate, DateTime? toDate, int transactionTypeId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var transactions = context.TransactionInternal
								.Include(x => x.User)
								.Where
								(x =>
									((fromDate != null && toDate != null) ? fromDate <= x.DateCreate && toDate >= x.DateCreate : true) &&
									x.User.Email.Contains(email) &&
									(orderId == 0 ? true : x.OrderId == orderId) &&
									(transactionTypeId == 0 ? true : x.TransactionInternalTypeId == transactionTypeId)
								)
								.OrderByDescending(x => x.DateCreate)
								.ToList();
				return transactions;

			}
		}

		internal int GetNumberTransactionInternalOfUser(long userId, long orderId, DateTime? fromDate, DateTime? toDate, int transactionInternalTypeId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var transactions = context.TransactionInternal
								.Include(x => x.User)
								.Where
								(x =>
									x.User.UserId == userId &&
									((fromDate != null && toDate != null) ? fromDate <= x.DateCreate && toDate >= x.DateCreate : true) &&
									(orderId == 0 ? true : x.OrderId == orderId) &&
									(transactionInternalTypeId == 0 ? true : x.TransactionInternalTypeId == transactionInternalTypeId)
								)
								.Count();
				return transactions;
			}
		}

		internal List<TransactionInternal> GetNumberTransactionInternalOfUser(long userId, long orderId, DateTime? fromDate, DateTime? toDate, int transactionInternalTypeId, int page)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var transactions = context.TransactionInternal
								.Include(x => x.User)
								.Where
								(x =>
									x.User.UserId == userId &&
									((fromDate != null && toDate != null) ? fromDate <= x.DateCreate && toDate >= x.DateCreate : true) &&
									(orderId == 0 ? true : x.OrderId == orderId) &&
									(transactionInternalTypeId == 0 ? true : x.TransactionInternalTypeId == transactionInternalTypeId)
								)
								.OrderByDescending(x => x.DateCreate)
								.ToList();
				return transactions;

			}
		}
	}
}
