﻿using DigitalFUHubApi.Comons;
using Quartz;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using DataAccess.DAOs;
using DigitalFUHubApi.Services;
using DTOs.MbBank;
using Comons;

namespace DigitalFUHubApi.Jobs
{
    public class HistoryDepositTransactionMbBankJob : IJob
	{
		private readonly IConfiguration configuration;
		private readonly MbBankService mbBankService;

		public HistoryDepositTransactionMbBankJob(IConfiguration configuration, MbBankService mbBankService)
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

			List<TransactionHistory> transactionHistoryCreditList = new List<TransactionHistory>();

			if (data.transactionHistoryList != null)
			{
				transactionHistoryCreditList = data.transactionHistoryList
					.Where(x => x.creditAmount != 0 && x.description.Contains(Constants.BANK_TRANSACTION_CODE_KEY)).ToList();
			}

			string? directoryPathStoreData = configuration["MbBank:DirectoryPathStoreDepositData"];
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
