using FuMarketAPI.Comons;
using Quartz;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using DTOs;
using DataAccess.DAOs;

namespace FuMarketAPI.Jobs
{
	public class HistoryTransactionMbBankJob : IJob
	{
		private readonly HttpClient client;
		private IConfigurationRoot Appsettings;
		public HistoryTransactionMbBankJob()
		{
			Appsettings = new ConfigurationBuilder()
							.SetBasePath(Directory.GetCurrentDirectory())
							.AddJsonFile("appsettings.json", true, true)
							.Build();
			client = new HttpClient();
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			client.DefaultRequestHeaders.Add("Authorization", "Basic " + Appsettings["MbBank:BasicAuthBase64"]);
		}

		public async Task Execute(IJobExecutionContext context)
		{
			MbBankRequestBodyHistoryTransactionDTO mbBank = new MbBankRequestBodyHistoryTransactionDTO()
			{
				accountNo = Appsettings["MbBank:AccountNo"],
				deviceIdCommon = Appsettings["MbBank:DeviceIdCommon"],
				refNo = Appsettings["MbBank:RefNo"],
				fromDate = DateTime.Now.AddDays(-5).ToString("dd/MM/yyyy"),
				sessionId = Appsettings["MbBank:SessionId"],
				toDate = DateTime.Now.ToString("dd/MM/yyyy")
			};

			var jsonData = JsonSerializer.Serialize(mbBank);
			var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
			var request = await client.PostAsync(Appsettings["MbBank:APIHistoryTransaction"], content);

			var respone = await request.Content.ReadAsStringAsync();

			var options = new JsonSerializerOptions();
			options.Converters.Add(new JsonSerializerDateTimeConverter());
			options.Converters.Add(new JsonSerializerIntConverter());

			var data = JsonSerializer.Deserialize<MbBankResponeHistoryTransactionDataDTO>(respone, options);

			if (data == null) return;

			List<TransactionHistory> transactionHistoryCreditList = new List<TransactionHistory>();

			if (data.transactionHistoryList != null)
			{
				transactionHistoryCreditList = data.transactionHistoryList
					.Where(x => x.creditAmount != 0).ToList();
			}

			if (data.result.responseCode == "00")// Get data success
			{
				string dataPreviousText = Util.ReadFile("Data/historyTransactionMbBank.json");
				List<TransactionHistory>? dataPrevious = new List<TransactionHistory>();
				if (!string.IsNullOrEmpty(dataPreviousText)) 
				{
					dataPrevious = JsonSerializer.Deserialize<List<TransactionHistory>>(dataPreviousText);
				}

				// Compare previous data with current data
				
				bool isSame = true;
				if (dataPrevious != null)
				{
					isSame = dataPrevious.SequenceEqual(transactionHistoryCreditList, new MbBankResponeHistoryTransactionDataEqualityComparer());
				}
				if (isSame) return;
				//Have new recharge info
				// Save new data into file
				Util.WriteFile("Data/historyTransactionMbBank.json", transactionHistoryCreditList);
				//Update DB
				DepositTransactionDAO.Instance.CheckingCreditTransactions(transactionHistoryCreditList);
			}

		}
	}
}
