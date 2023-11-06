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
	public class TransactionCoinRepository : ITransactionCoinRepository
	{
		public List<TransactionCoin> GetHistoryTransactionInternal(long orderId, string email, DateTime fromDate, DateTime toDate, int transactionCoinTypeId) => TransactionCoinDAO.Instance.GetHistoryTransactionInternal(orderId, email, fromDate, toDate, transactionCoinTypeId);


	}
}
