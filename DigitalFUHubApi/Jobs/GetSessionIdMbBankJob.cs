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
		private readonly MbBankService mbBankService;
		private readonly OpticalCharacterRecognitionService imageService;

		public GetSessionIdMbBankJob(MbBankService mbBankService, OpticalCharacterRecognitionService imageService)
		{
			this.mbBankService = mbBankService;
			this.imageService = imageService;
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
				var captchaRaw = imageService.ExtractTextFromImage(base64CaptchaImage).Trim();
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
				var captchaRaw = await imageService.ExtractTextFromImageAzureComputerVision(base64CaptchaImage) ;
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
			#endregion

			// update sessionId in to json file
			MbBankAccount? mbBankAccount = mbBankService.GetMbBankAccount();
			if (mbBankAccount == null) return;
			mbBankAccount.SessionId = sessionId;

			Util.WriteFile(Constants.MB_BANK_DIRECTORY_PATH_STORE_ACCOUNT_DATA, mbBankAccount);

		}
	}
}
