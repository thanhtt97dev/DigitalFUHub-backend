using BusinessObject;
using DigitalFUHubApi.Comons;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using DTOs.Bank;
using DTOs.MbBank;
using Comons;

namespace DigitalFUHubApi.Services
{
    public class MbBankService
	{
		private HttpClient client;

		public MbBankService()
		{
			client = new HttpClient();
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			client.DefaultRequestHeaders.Add("Authorization", "Basic " + Constants.MB_BANK_BASIC_AUTH_BASE64);
		}

		#region Get Mb Bank account info
		public MbBankAccount? GetMbBankAccount()
		{
			MbBankAccount? mbBankAccount = new MbBankAccount();
			string mbBankAccountDataJson = Util.ReadFile(Constants.MB_BANK_DIRECTORY_PATH_STORE_ACCOUNT_DATA);
			if (!string.IsNullOrEmpty(mbBankAccountDataJson))
			{
				mbBankAccount = JsonSerializer.Deserialize<MbBankAccount>(mbBankAccountDataJson);
			}
			return mbBankAccount;
		}
		#endregion

		#region Get History Transaction
		public async Task<MbBankResponeHistoryTransactionDataDTO?> GetHistoryTransaction()
		{
			MbBankAccount? mbBankAccount = GetMbBankAccount();
			if (mbBankAccount == null) return null;

			MbBankRequestBodyHistoryTransactionDTO mbBank = new MbBankRequestBodyHistoryTransactionDTO()
			{
				accountNo = mbBankAccount.AccountNo,
				deviceIdCommon = mbBankAccount.DeviceIdCommon,
				refNo = mbBankAccount.RefNo,
				fromDate = DateTime.Now.AddDays(-2).ToString("dd/MM/yyyy"),
				sessionId = mbBankAccount.SessionId,
				toDate = DateTime.Now.ToString("dd/MM/yyyy")
			};

			var jsonData = JsonSerializer.Serialize(mbBank);
			var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
			var request = await client.PostAsync(Constants.MB_BANK_API_HISTORY_TRANSACTION, content);

			var respone = await request.Content.ReadAsStringAsync();

			var options = new JsonSerializerOptions();
			options.Converters.Add(new JsonSerializerDateTimeConverter());
			options.Converters.Add(new JsonSerializerIntConverter());

			var data = JsonSerializer.Deserialize<MbBankResponeHistoryTransactionDataDTO>(respone, options);

			return data;
		}
		#endregion

		#region Inquiry Account Name
		public async Task<MbBankResponse?> InquiryAccountName(BankInquiryAccountNameRequestDTO bankInquiryAccountNameRequestDTO)
		{
			MbBankAccount? mbBankAccount = GetMbBankAccount();
			if (mbBankAccount == null) return null;

			MbBankRequestBodyInquiryAccountNameDTO mbBank = new MbBankRequestBodyInquiryAccountNameDTO()
			{
				bankCode = bankInquiryAccountNameRequestDTO.BankId,
				creditAccount = bankInquiryAccountNameRequestDTO.CreditAccount,
				creditAccountType = "ACCOUNT",
				debitAccount = mbBankAccount.AccountNo,
				deviceIdCommon = mbBankAccount.DeviceIdCommon,
				refNo = mbBankAccount.RefNo,
				remark = string.Empty,
				sessionId = mbBankAccount.SessionId,
				type = bankInquiryAccountNameRequestDTO.BankId == Constants.BANK_ID_MB_BANK ? "INHOUSE" : "FAST"
			};

			var jsonData = JsonSerializer.Serialize(mbBank);
			var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
			var request = await client.PostAsync(Constants.MB_BANK_API_INQUIRY_ACCOUNT_NAME, content);

			var respone = await request.Content.ReadAsStringAsync();

			var options = new JsonSerializerOptions();
			options.Converters.Add(new JsonSerializerDateTimeConverter());
			options.Converters.Add(new JsonSerializerIntConverter());

			var data = JsonSerializer.Deserialize<MbBankResponeInquiryAccountNameDTO>(respone, options);

			if (data == null) return null;

			MbBankResponse mbBankResponse = new MbBankResponse()
			{
				Code = data.result.responseCode,
				Result = data.benName
			};

			return mbBankResponse;
		}
		#endregion

		#region Get Captcha Image
		public async Task<MbBankResponse?> GetCaptchaImage()
		{
			MbBankAccount? mbBankAccount = GetMbBankAccount();
			if (mbBankAccount == null) return null;

			MbBankRequestBodyGetCaptchaImageDTO body = new MbBankRequestBodyGetCaptchaImageDTO()
			{
				deviceIdCommon = mbBankAccount.DeviceIdCommon,
				refNo = mbBankAccount.RefNo,
				sessionId = string.Empty
			};

			var jsonData = JsonSerializer.Serialize(body);
			var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
			var request = await client.PostAsync(Constants.MB_BANK_API_GET_CAPTCHA_IMAGE, content);

			var respone = await request.Content.ReadAsStringAsync();

			var options = new JsonSerializerOptions();
			options.Converters.Add(new JsonSerializerDateTimeConverter());
			options.Converters.Add(new JsonSerializerIntConverter());

			var data = JsonSerializer.Deserialize<MbBankResponseGetCaptchaImageDTO>(respone, options);

			if (data == null) return null;

			MbBankResponse mbBankResponse = new MbBankResponse()
			{
				Code = data.result.responseCode,
				Result = data.imageString
			};

			return mbBankResponse;
		}
		#endregion

		#region Login
		public async Task<MbBankResponse?> Login(string captcha)
		{
			MbBankAccount? mbBankAccount = GetMbBankAccount();
			if (mbBankAccount == null) return null;

			MbBankRequestBodyDoLoginDTO body = new MbBankRequestBodyDoLoginDTO()
			{
				captcha = captcha,
				deviceIdCommon = mbBankAccount.DeviceIdCommon,
				ibAuthen2faString = mbBankAccount.IbAuthen2faString,
				password = mbBankAccount.Password,
				refNo = mbBankAccount.RefNo,
				sessionId = null,
				userId = mbBankAccount.AccountNo,
			};

			var jsonData = JsonSerializer.Serialize(body);
			var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
			var request = await client.PostAsync(Constants.MB_BANK_API_LOGIN, content);

			var respone = await request.Content.ReadAsStringAsync();

			var options = new JsonSerializerOptions();
			options.Converters.Add(new JsonSerializerDateTimeConverter());
			options.Converters.Add(new JsonSerializerIntConverter());

			var data = JsonSerializer.Deserialize<MbBankResponseLoginDTO>(respone, options);

			if (data == null) return null;

			MbBankResponse mbBankResponse = new MbBankResponse()
			{
				Code = data.result.responseCode,
				Result = data.sessionId
			};

			return mbBankResponse;
		}
		#endregion
	}
}
