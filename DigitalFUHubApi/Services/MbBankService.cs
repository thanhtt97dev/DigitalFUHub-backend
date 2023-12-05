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
		private  HttpClient client;

		public MbBankService()
		{
			client = new HttpClient();
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			client.DefaultRequestHeaders.Add("Authorization", "Basic " + MbBankAccountData.BasicAuthBase64);
		}

		#region Get History Transaction
		public async Task<MbBankResponeHistoryTransactionDataDTO?> GetHistoryTransaction()
		{
			MbBankRequestBodyHistoryTransactionDTO mbBank = new MbBankRequestBodyHistoryTransactionDTO()
			{
				accountNo = MbBankAccountData.AccountNo,
				deviceIdCommon = MbBankAccountData.DeviceIdCommon,
				refNo = MbBankAccountData.RefNo,
				fromDate = DateTime.Now.AddDays(-5).ToString("dd/MM/yyyy"),
				sessionId = MbBankAccountData.SessionId,
				toDate = DateTime.Now.ToString("dd/MM/yyyy")
			};

			var jsonData = JsonSerializer.Serialize(mbBank);
			var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
			var request = await client.PostAsync(MbBankAccountData.ApiHistoryTransaction, content);

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
			MbBankRequestBodyInquiryAccountNameDTO mbBank = new MbBankRequestBodyInquiryAccountNameDTO()
			{
				bankCode = bankInquiryAccountNameRequestDTO.BankId,
				creditAccount = bankInquiryAccountNameRequestDTO.CreditAccount,
				creditAccountType = "ACCOUNT",
				debitAccount = MbBankAccountData.AccountNo,
				deviceIdCommon = MbBankAccountData.DeviceIdCommon,
				refNo = MbBankAccountData.RefNo,
				remark = string.Empty,
				sessionId = MbBankAccountData.SessionId,
				type = bankInquiryAccountNameRequestDTO.BankId == Constants.BANK_ID_MB_BANK ? "INHOUSE" : "FAST"
			};

			var jsonData = JsonSerializer.Serialize(mbBank);
			var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
			var request = await client.PostAsync(MbBankAccountData.ApiInquiryAccountName, content);

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
			MbBankRequestBodyGetCaptchaImageDTO body = new MbBankRequestBodyGetCaptchaImageDTO()
			{
				deviceIdCommon = MbBankAccountData.DeviceIdCommon,
				refNo = MbBankAccountData.RefNo,
				sessionId = string.Empty
			};

			var jsonData = JsonSerializer.Serialize(body);
			var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
			var request = await client.PostAsync(MbBankAccountData.ApiGetCaptchaImage, content);

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
			MbBankRequestBodyDoLoginDTO body = new MbBankRequestBodyDoLoginDTO()
			{
				captcha = captcha,
				deviceIdCommon = MbBankAccountData.DeviceIdCommon,
				password = MbBankAccountData.Password,
				refNo = MbBankAccountData.RefNo,
				sessionId = null,
				userId = MbBankAccountData.AccountNo,
			};

			var jsonData = JsonSerializer.Serialize(body);
			var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
			var request = await client.PostAsync(MbBankAccountData.ApiLogin, content);

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
