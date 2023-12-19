using BusinessObject.Entities;
using DataAccess.DAOs;
using DataAccess.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
	public class TransactionInternalRepository : ITransactionInternalRepository
	{
		public void AddTransactionsForOrderConfirmed(Order order) => TransactionInternalDAO.Instance.AddTransactionsForOrderConfirmed(order);
		public List<TransactionInternal> GetDataReportTransactionInternal(long orderId, string email, DateTime? fromDate, DateTime? toDate, int transactionInternalTypeId) => TransactionInternalDAO.Instance.GetDataReportTransactionInternal(orderId, email, fromDate, toDate, transactionInternalTypeId);
		public List<TransactionInternal> GetHistoryTransactionInternal(long orderId, string email, DateTime? fromDate, DateTime? toDate, int transactionTypeId, int page) => TransactionInternalDAO.Instance.GetHistoryTransactionInternal(orderId, email, fromDate, toDate, transactionTypeId, page);
		public int GetNumberTransactionInternal(long orderId, string email, DateTime? fromDate, DateTime? toDate, int transactionTypeId) => TransactionInternalDAO.Instance.GetNumberTransactionInternal(orderId, email, fromDate, toDate, transactionTypeId);
		public int GetNumberTransactionInternalOfUser(long userId, long orderId, DateTime? fromDate, DateTime? toDate, int transactionInternalTypeId) => TransactionInternalDAO.Instance.GetNumberTransactionInternalOfUser(userId, orderId, fromDate, toDate, transactionInternalTypeId);

		public List<TransactionInternal> GetHistoryTransactionInternalOfUser(long userId, long orderId, DateTime? fromDate, DateTime? toDate, int transactionInternalTypeId, int page) => TransactionInternalDAO.Instance.GetNumberTransactionInternalOfUser(userId, orderId, fromDate, toDate, transactionInternalTypeId, page);
	}
}
