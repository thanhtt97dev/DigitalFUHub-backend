using Comons;
using DataAccess.IRepositories;
using Quartz;

namespace DigitalFUHubApi.Jobs
{
	public class UpdateStatusRequestDepositMoneyToExpired : IJob
	{
		private readonly IBankRepository bankRepository;

		public UpdateStatusRequestDepositMoneyToExpired(IBankRepository bankRepository)
		{
			this.bankRepository = bankRepository;
		}

		public Task Execute(IJobExecutionContext context)
		{
			bankRepository.UpdateStatusRequestDepositMoneyToExpired(Constants.NUMBER_DAY_DEPOSIT_REQUEST_EXPIRED);
			return Task.CompletedTask;
		}
	}
}
