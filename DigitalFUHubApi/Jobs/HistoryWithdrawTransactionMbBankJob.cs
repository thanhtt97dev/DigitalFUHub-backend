using DataAccess.DAOs;
using DigitalFUHubApi.Comons;
using DigitalFUHubApi.Services;
using DTOs.MbBank;
using Quartz;
using System.Text.Json;
using Comons;

namespace DigitalFUHubApi.Jobs
{
	public class HistoryWithdrawTransactionMbBankJob : IJob
	{
		public Task Execute(IJobExecutionContext context)
		{
			//get folder name history transaction data
			string? directoryPathStoreData = MbBankAccountData.DirectoryPathStoreData;
			if (directoryPathStoreData == null)
				return Task.CompletedTask;

			//get data history transaction
			string dataHistoryTransactionJson =  Util.ReadFile(directoryPathStoreData);
			if (string.IsNullOrEmpty(dataHistoryTransactionJson))
				return Task.CompletedTask;

			List<TransactionHistory>? data = new List<TransactionHistory>();
			data = JsonSerializer.Deserialize<List<TransactionHistory>>(dataHistoryTransactionJson);

			// get debit data
			List<TransactionHistory> transactionHistoryDebitList = new List<TransactionHistory>();
			if (data != null && data.Count != 0)
			{
				transactionHistoryDebitList = data
					.Where(x => x.debitAmount != 0 && x.description.Contains(Constants.BANK_TRANSACTION_CODE_KEY)).ToList();
			}

			// get previous data debit transaction
			string? directoryPathStoreWithdrawData = MbBankAccountData.DirectoryPathStoreDepositData;
			if (directoryPathStoreWithdrawData == null)
				return Task.CompletedTask;

			string dataPreviousDebitTransactionJson = Util.ReadFile(directoryPathStoreWithdrawData);
			List<TransactionHistory>? dataPrevious = new List<TransactionHistory>();
			if (!string.IsNullOrEmpty(dataPreviousDebitTransactionJson))
			{
				dataPrevious = JsonSerializer.Deserialize<List<TransactionHistory>>(dataPreviousDebitTransactionJson);
			}

			// Compare previous data with current data
			bool isSame = true;
			if (dataPrevious != null)
			{
				isSame = dataPrevious.SequenceEqual(transactionHistoryDebitList,
					new MbBankResponeHistoryTransactionDataEqualityComparer());
			}
			if (isSame)
				return Task.CompletedTask;

			//Have new debit info
			// Save new data into file
			Util.WriteFile(directoryPathStoreWithdrawData, transactionHistoryDebitList);

			//Update DB
			BankDAO.Instance.CheckingDebitTransactions(transactionHistoryDebitList);

			return Task.CompletedTask;
		}
	}
}
