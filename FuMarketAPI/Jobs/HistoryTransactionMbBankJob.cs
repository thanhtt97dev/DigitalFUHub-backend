using FuMarketAPI.Comons;
using Quartz;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using DTOs;
using DataAccess.DAOs;
using FuMarketAPI.Services;

namespace FuMarketAPI.Jobs
{
	public class HistoryTransactionMbBankJob : IJob
	{
		private readonly IConfiguration configuration;
		private readonly MbBankService mbBankService;

		public HistoryTransactionMbBankJob(IConfiguration configuration, MbBankService mbBankService)
		{
			this.configuration = configuration;
			this.mbBankService = mbBankService;
		}

		public async Task Execute(IJobExecutionContext context)
		{
			//return;
			var data = await mbBankService.GetHistoryTransaction();

			if (data == null) return;

			List<TransactionHistory> transactionHistoryCreditList = new List<TransactionHistory>();

			if (data.transactionHistoryList != null)
			{
				transactionHistoryCreditList = data.transactionHistoryList
					.Where(x => x.creditAmount != 0).ToList();
			}

			if (data.result.responseCode == "00")// Get data success
			{
				string? directoryPathStoreData = configuration["MbBank:DirectoryPathStoreData"];
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
					isSame = dataPrevious.SequenceEqual(transactionHistoryCreditList, 
						new MbBankResponeHistoryTransactionDataEqualityComparer());
				}
				if (isSame) return;
				//Have new recharge info
				// Save new data into file
				Util.WriteFile(directoryPathStoreData, transactionHistoryCreditList);
				//Update DB
				BankDAO.Instance.CheckingCreditTransactions(transactionHistoryCreditList);
			}

		}
	}
}
