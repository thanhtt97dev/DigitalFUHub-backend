using BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.IRepositories
{
	public interface ITransactionCoinRepository
	{
		List<TransactionCoin> GetHistoryTransactionInternal(long orderId, string email, DateTime fromDate, DateTime toDate, int transactionCoinTypeId);
	}
}
