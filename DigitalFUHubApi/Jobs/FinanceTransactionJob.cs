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
		private readonly FinanceTransactionService financeTransactionService;

		public FinanceTransactionJob(FinanceTransactionService financeTransactionService)
		{
			this.financeTransactionService = financeTransactionService;
		}

		public Task Execute(IJobExecutionContext context)
		{
			financeTransactionService.HandleFinanceTransactions();
			return Task.CompletedTask;
		}
	}
}
