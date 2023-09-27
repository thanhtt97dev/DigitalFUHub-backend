using DataAccess.DAOs;
using DigitalFUHubApi.Comons;
using DigitalFUHubApi.Services;
using DTOs.MbBank;
using Quartz;
using System.Text.Json;

namespace DigitalFUHubApi.Jobs
{
	public class HistoryWithdrawTransactionMbBankJob : IJob
	{
		private readonly IConfiguration configuration;
		private readonly MbBankService mbBankService;
		public HistoryWithdrawTransactionMbBankJob(IConfiguration configuration, MbBankService mbBankService)
		{
			this.configuration = configuration;
			this.mbBankService = mbBankService;
		}

		public async Task Execute(IJobExecutionContext context)
		{
			//return;
			var data = await mbBankService.GetHistoryTransaction();
			if (data == null) return;
			if (data.result.responseCode != "00") return;

			List<TransactionHistory> transactionHistoryDebitList = new List<TransactionHistory>();
			if (data.transactionHistoryList != null)
			{
				transactionHistoryDebitList = data.transactionHistoryList
					.Where(x => x.debitAmount != 0 && x.description.Contains(Constants.BANK_TRANSACTION_CODE_KEY)).ToList();
			}

			string? directoryPathStoreData = configuration["MbBank:DirectoryPathStoreWithdrawData"];
			if (directoryPathStoreData == null) return;

			string dataPreviousText = Util.ReadFile(directoryPathStoreData);

			List<TransactionHistory>? dataPrevious = new List<TransactionHistory>();
			if (!string.IsNullOrEmpty(dataPreviousText))
			{
				dataPrevious = JsonSerializer.Deserialize<List<TransactionHistory>>(dataPreviousText);
			}

			// Compare previous data with current data
			bool isSame = true;
			if (dataPrevious != null)
			{
				isSame = dataPrevious.SequenceEqual(transactionHistoryDebitList,
					new MbBankResponeHistoryTransactionDataEqualityComparer());
			}
			if (isSame) return;

			//Have new debit info
			// Save new data into file
			Util.WriteFile(directoryPathStoreData, transactionHistoryDebitList);

			//Update DB
			BankDAO.Instance.CheckingDebitTransactions(transactionHistoryDebitList);
		}
	}
}
