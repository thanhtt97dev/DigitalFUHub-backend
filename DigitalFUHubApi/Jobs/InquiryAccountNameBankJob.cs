using Comons;
using DigitalFUHubApi.Services;
using DTOs.Bank;
using Quartz;

namespace DigitalFUHubApi.Jobs
{
	public class InquiryAccountNameBankJob : IJob
	{
		private readonly IConfiguration configuration;
		private readonly MbBankService mbBankService;

		public InquiryAccountNameBankJob(IConfiguration configuration, MbBankService mbBankService)
		{
			this.configuration = configuration;
			this.mbBankService = mbBankService;
		}

		public async Task Execute(IJobExecutionContext context)
		{
			BankInquiryAccountNameRequestDTO requestBody = new BankInquiryAccountNameRequestDTO
			{
				BankId = Constants.BANK_ID_MB_BANK,
				CreditAccount = configuration["MbBank:FirstBankAccount:AccountNo"] ?? string.Empty,
			};
			await mbBankService.InquiryAccountName(requestBody);
		}
	}
}
