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
		public List<TransactionCoin> GetDataReportTransactionCoin(long orderId, string email, DateTime? fromDate, DateTime? toDate, int transactionCoinTypeId) => TransactionCoinDAO.Instance.GetDataReportTransactionCoin(orderId, email, fromDate, toDate, transactionCoinTypeId);
		public List<TransactionCoin> GetHistoryTransactionCoin(long orderId, string email, DateTime? fromDate, DateTime? toDate, int transactionCoinTypeId, int page) => TransactionCoinDAO.Instance.GetHistoryTransactionCoin(orderId, email, fromDate, toDate, transactionCoinTypeId, page);

		public int GetNumberTransactionCoin(long orderId, string email, DateTime? fromDate, DateTime? toDate, int transactionCoinTypeId) => TransactionCoinDAO.Instance.GetNumberTransactionCoin(orderId, email, fromDate, toDate, transactionCoinTypeId);

		public long GetNumberCoinUsedOrdersCurrentMonth()
		=> TransactionCoinDAO.Instance.GetNumberCoinUsedOrdersCurrentMonth();

		public long GetNumberCoinReceiveCurrentMonth()
		=> TransactionCoinDAO.Instance.GetNumberCoinReceiveCurrentMonth();
	}
}
