using BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.IRepositories
{
	public interface ITransactionRepository
	{
		void AddTransactionsForOrderConfirmed(Order order);
		List<TransactionInternal> GetHistoryTransactionInternal(long orderId, string email, DateTime fromDate, DateTime toDate, int transactionTypeId);
	}
}
