using Comons;
using BusinessObject.Entities;
using DigitalFUHubApi.Comons;
using DigitalFUHubApi.Managers.IRepositories;

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
			Queue<FinanceTransaction> financeTransactions = financeTransactionManager.GetQueuesAndClearItemGeted();
			while (financeTransactions.Count > 0)
			{
				var financeTransaction = financeTransactions.Dequeue();
				switch (financeTransaction.Type)
				{
					case Constants.FinanceTransactionType.Order:

						break;
					case Constants.FinanceTransactionType.Deposit:

						break;
					case Constants.FinanceTransactionType.Withdraw:

						break;
					case Constants.FinanceTransactionType.RegisterSeller:

						break;
				}
			}
		}

		public void AddDepositTransactions(List<DepositTransaction> depositTransactions)
		{
			var financeTransactions = new List<FinanceTransaction>();	
			foreach (var depositTransaction in depositTransactions)
			{
				var transaction = new FinanceTransaction()
				{
					UserId = depositTransaction.UserId,
					Type = Constants.FinanceTransactionType.Deposit,
					ForeignId = depositTransaction.DepositTransactionId,
					Amount = depositTransaction.Amount
				};
				financeTransactions.Add(transaction);
			}

			financeTransactionManager.Enqueues(financeTransactions);
		}

		
	}
}
