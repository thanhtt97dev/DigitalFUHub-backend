using Comons;
using DataAccess.DAOs;
using DigitalFUHubApi.Comons;
using DigitalFUHubApi.Services;
using DTOs.MbBank;
using Quartz;

namespace DigitalFUHubApi.Jobs
{
	public class GetSessionIdMbBankJob : IJob
	{
		private readonly OpticalCharacterRecognitionService opticalCharacterRecognitionService;
		private readonly MbBankService mbBankService;

		public GetSessionIdMbBankJob(MbBankService mbBankService, OpticalCharacterRecognitionService opticalCharacterRecognitionService)
		{
			this.mbBankService = mbBankService;
			this.opticalCharacterRecognitionService = opticalCharacterRecognitionService;
		}

		public async Task Execute(IJobExecutionContext context)
		{
			return;
			string base64CaptchaImage = string.Empty;
			string sessionId = string.Empty;

			#region TesseractEngine
			/*
			do
			{
				//get captcha
				do
				{
					var responseGetCaptcha = await mbBankService.GetCaptchaImage();
					if (responseGetCaptcha == null) continue;

					if (responseGetCaptcha.Code == Constants.MB_BANK_RESPONE_CODE_SUCCESS)
					{
						base64CaptchaImage = (string)responseGetCaptcha.Result;
					}
				}
				while (string.IsNullOrEmpty(base64CaptchaImage));

				//login
				var captchaRaw = opticalCharacterRecognitionService.ExtractTextFromImage(base64CaptchaImage).Trim();
				if (captchaRaw == null) return;

				var captcha = String.Concat(captchaRaw.Where(c => !Char.IsWhiteSpace(c)));

				var response = await mbBankService.Login(captcha);
				if (response == null) continue;

				if (response.Code == Constants.MB_BANK_RESPONE_CODE_SUCCESS)
				{
					sessionId = (string)response.Result;
				}
			}
			while (string.IsNullOrEmpty(sessionId));
			*/
			#endregion

			#region Azure Computer Vision
			do
			{
				//get captcha
				do
				{
					var responseGetCaptcha = await mbBankService.GetCaptchaImage();
					if (responseGetCaptcha == null) continue;

					if (responseGetCaptcha.Code == Constants.MB_BANK_RESPONE_CODE_SUCCESS)
					{
						base64CaptchaImage = (string)responseGetCaptcha.Result;
					}
				}
				while (string.IsNullOrEmpty(base64CaptchaImage));

				//login
				try
				{
					byte[]? captchaImageStream = opticalCharacterRecognitionService.GetClarifyCaptchaImageByBase64(base64CaptchaImage);
					if(captchaImageStream == null) continue;	

					var captchaRaw = await opticalCharacterRecognitionService.ExtractTextFromImageAzureComputerVision(captchaImageStream);
					if (captchaRaw == null) return;

					var captcha = String.Concat(captchaRaw.Where(c => !Char.IsWhiteSpace(c)));

					var response = await mbBankService.Login(captcha);
					if (response == null) continue;

					if (response.Code == Constants.MB_BANK_RESPONE_CODE_SUCCESS)
					{
						sessionId = (string)response.Result;
					}
				}
				catch (Exception ex)
				{
					var message = new
					{
						Error = ex.Message,
					};
					Util.WriteFile("Data/err.json", message);
				}
				
			}
			while (string.IsNullOrEmpty(sessionId));
			#endregion

			// update sessionId in to json file
			MbBankAccount? mbBankAccount = mbBankService.GetMbBankAccount();
			if (mbBankAccount == null) return;
			mbBankAccount.SessionId = sessionId;

			Util.WriteFile(Constants.MB_BANK_DIRECTORY_PATH_STORE_ACCOUNT_DATA, mbBankAccount);

		}
	}
}
