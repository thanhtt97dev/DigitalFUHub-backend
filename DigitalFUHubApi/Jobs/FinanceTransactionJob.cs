using Comons;
using DataAccess.DAOs;
using DigitalFUHubApi.Comons;
using DigitalFUHubApi.Managers.IRepositories;
using DigitalFUHubApi.Services;
using Quartz;

namespace DigitalFUHubApi.Jobs
{
	public class FinanceTransactionJob : IJob
	{
		private Queue<FinanceTransaction> data = new Queue<FinanceTransaction>();
		private readonly FinanceTransactionService financeTransactionService;

		public FinanceTransactionJob(Queue<FinanceTransaction> data, FinanceTransactionService financeTransactionService)
		{
			this.data = data;
			this.financeTransactionService = financeTransactionService;
		}

		public Task Execute(IJobExecutionContext context)
		{
			if(data.Count == 0) return Task.CompletedTask;

			financeTransactionService.HandleFinanceTransactions();

			return Task.CompletedTask;
		}
	}
}
