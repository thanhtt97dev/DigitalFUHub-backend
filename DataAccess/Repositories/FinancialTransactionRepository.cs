using BusinessObject;
using DataAccess.DAOs;
using DataAccess.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
	public class FinancialTransactionRepository : IFinancialTransactionRepository
	{
		public void CreateDepositTransaction(DepositTransaction transaction) => FinancialTransactionDAO.Instance.CreateDepositTransaction(transaction);
	}
}
