using Comons;
using BusinessObject.Entities;
using DigitalFUHubApi.Comons;
using DigitalFUHubApi.Managers.IRepositories;
using DataAccess.DAOs;

namespace DigitalFUHubApi.Services
{
	public class FinanceTransactionService
	{
		private readonly IFinanceTransactionManager financeTransactionManager;

		public FinanceTransactionService(IFinanceTransactionManager financeTransactionManager)
		{
			this.financeTransactionManager = financeTransactionManager;
		}

		public void HandleFinanceTransactions()
		{
			//copy
			Queue<FinanceTransaction> financeTransactions = financeTransactionManager.GetData();
			while (financeTransactions.Count > 0)
			{
				var financeTransaction = financeTransactions.Dequeue();
				switch (financeTransaction.Type)
				{
					case Constants.FinanceTransactionType.Order:
						
						break;
					case Constants.FinanceTransactionType.Deposit:
						HandleFinanceTransactionTypeDeposit(financeTransaction);
						break;
					case Constants.FinanceTransactionType.Withdraw:

						break;
					case Constants.FinanceTransactionType.RegisterSeller:

						break;
				}
			}
		}

		#region AddDepositTransactions 
		public void AddDepositTransactions(List<DepositTransaction> depositTransactions)
		{
			var financeTransactions = new List<FinanceTransaction>();
			foreach (var depositTransaction in depositTransactions)
			{
				var transaction = new FinanceTransaction()
				{
					UserId = depositTransaction.UserId,
					Type = Constants.FinanceTransactionType.Deposit,
					Data = depositTransaction
				};
				financeTransactions.Add(transaction);
			}

			financeTransactionManager.Enqueues(financeTransactions);
		}
		#endregion

		#region HandleFinanceTransactionTypeDeposit
		private void HandleFinanceTransactionTypeDeposit(FinanceTransaction financeTransaction)
		{
			DepositTransaction? depositTransaction = financeTransaction.Data as DepositTransaction;
			if (depositTransaction == null) return;
			try
			{
				BankDAO.Instance.UpdateDepositTransactionPaid(depositTransaction);
			}
			catch (Exception)
			{
				Util.WirteToLogFile($"Error: deposit transaction ID: ${depositTransaction.DepositTransactionId}");
			}
		}
		#endregion




	}
}
