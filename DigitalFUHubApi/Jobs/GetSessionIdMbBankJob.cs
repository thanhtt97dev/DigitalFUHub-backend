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
		private readonly IConfiguration configuration;
		private readonly MbBankService mbBankService;
		private readonly OpticalCharacterRecognitionService imageService;

		public GetSessionIdMbBankJob(IConfiguration configuration, MbBankService mbBankService, OpticalCharacterRecognitionService imageService)
		{
			this.configuration = configuration;
			this.mbBankService = mbBankService;
			this.imageService = imageService;
		}

		public async Task Execute(IJobExecutionContext context)
		{
			return;
			string base64CaptchaImage = string.Empty;
			string sessionId = string.Empty;

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
				imageService.ClarifyCaptchaImage(base64CaptchaImage);
				var captchaRaw = imageService.GetCaptchaInImage().Trim();
				var captcha = String.Concat(captchaRaw.Where(c => !Char.IsWhiteSpace(c)));

				var response = await mbBankService.Login(captcha);
				if (response == null) continue;

				if (response.Code == Constants.MB_BANK_RESPONE_CODE_SUCCESS)
				{
					sessionId = (string)response.Result;
				}
			}
			while (string.IsNullOrEmpty(sessionId));

			// update sessionId in appsettings
			configuration["MbBank:SessionId"] = sessionId;
		}
	}
}
