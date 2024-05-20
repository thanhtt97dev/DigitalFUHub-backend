using Comons;
using DataAccess.DAOs;
using DigitalFUHubApi.Comons;
using DigitalFUHubApi.Services;
using DTOs.MbBank;
using Quartz;
using System.Text.Json;

namespace DigitalFUHubApi.Jobs
{
	public class HistoryTransactionMbBankJob : IJob
	{
		private readonly MbBankService mbBankService;

		public HistoryTransactionMbBankJob(MbBankService mbBankService)
		{
			this.mbBankService = mbBankService;
		}

		public async Task Execute(IJobExecutionContext context)
		{
			return;
			var data = await mbBankService.GetHistoryTransaction();

			if (data == null) return;
			if (data.result.responseCode != Constants.MB_BANK_RESPONE_CODE_SUCCESS) return;

			List<TransactionHistory> transactionHistoryList = new List<TransactionHistory>();
			if (data.transactionHistoryList != null)
			{
				transactionHistoryList = data.transactionHistoryList
					.Where(x => x.description.Contains(Constants.BANK_TRANSACTION_CODE_KEY)).ToList();
			}

			string? directoryPathStoreData = Constants.MB_BANK_DIRECTORY_PATH_STORE_TRANSACTION_DATA;
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
				isSame = dataPrevious.SequenceEqual(transactionHistoryList,
					new MbBankResponeHistoryTransactionDataEqualityComparer());
			}
			if (isSame) return;
			//Have new recharge info
			// Save new data into file
			Util.WriteFile(directoryPathStoreData, transactionHistoryList);
		}
	}
}
