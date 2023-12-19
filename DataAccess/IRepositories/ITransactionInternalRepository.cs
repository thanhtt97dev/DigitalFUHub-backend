using BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.IRepositories
{
	public interface ITransactionInternalRepository
	{
		void AddTransactionsForOrderConfirmed(Order order);
		List<TransactionInternal> GetDataReportTransactionInternal(long orderId, string email, DateTime? fromDate, DateTime? toDate, int transactionInternalTypeId);
		List<TransactionInternal> GetHistoryTransactionInternal(long orderId, string email, DateTime? fromDate, DateTime? toDate, int transactionTypeId, int page);
		int GetNumberTransactionInternal(long orderId, string email, DateTime? fromDate, DateTime? toDate, int transactionInternalTypeId);
		int GetNumberTransactionInternalOfUser(long userId, long orderId, DateTime? fromDate, DateTime? toDate, int transactionInternalTypeId);
		List<TransactionInternal> GetHistoryTransactionInternalOfUser(long userId, long orderId, DateTime? fromDate, DateTime? toDate, int transactionInternalTypeId, int page);
	}
}
