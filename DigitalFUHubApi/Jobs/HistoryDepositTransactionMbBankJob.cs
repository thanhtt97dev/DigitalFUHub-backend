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

		public Task Execute(IJobExecutionContext context)
		{
			//get folder name history transaction data
			string? directoryPathStoreData = configuration["MbBank:DirectoryPathStoreData"];
			if (directoryPathStoreData == null)
				return Task.CompletedTask;

			//get data history transaction
			string dataHistoryTransactionJson = Util.ReadFile(directoryPathStoreData);
			if (string.IsNullOrEmpty(dataHistoryTransactionJson))
				return Task.CompletedTask;

			List<TransactionHistory>? data = new List<TransactionHistory>();
			data = JsonSerializer.Deserialize<List<TransactionHistory>>(dataHistoryTransactionJson);

			// get credit data
			List<TransactionHistory> transactionHistoryCreditList = new List<TransactionHistory>();
			if (data != null && data.Count != 0)
			{
				transactionHistoryCreditList = data
					.Where(x => x.creditAmount != 0 && x.description.Contains(Constants.BANK_TRANSACTION_CODE_KEY)).ToList();
			}

			// get previous data debit transaction
			string? directoryPathStoreDepositData = configuration["MbBank:DirectoryPathStoreDepositData"];
			if (directoryPathStoreDepositData == null)
				return Task.CompletedTask;

			string dataPreviousCreditTransactionJson = Util.ReadFile(directoryPathStoreDepositData);
			List<TransactionHistory>? dataPrevious = new List<TransactionHistory>();
			if (!string.IsNullOrEmpty(dataPreviousCreditTransactionJson))
			{
				dataPrevious = JsonSerializer.Deserialize<List<TransactionHistory>>(dataPreviousCreditTransactionJson);
			}

			// Compare previous data with current data
			bool isSame = true;
			if (dataPrevious != null)
			{
				isSame = dataPrevious.SequenceEqual(transactionHistoryCreditList,
					new MbBankResponeHistoryTransactionDataEqualityComparer());
			}
			if (isSame) return Task.CompletedTask;
			//Have new recharge info
			// Save new data into file
			Util.WriteFile(directoryPathStoreDepositData, transactionHistoryCreditList);
			//Update DB
			BankDAO.Instance.CheckingCreditTransactions(transactionHistoryCreditList);

			return Task.CompletedTask;
		}
	}
}
