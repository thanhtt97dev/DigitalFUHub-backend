using Google.Authenticator;
using System.Drawing;
using ZXing;
using ZXing.QrCode.Internal;
using ZXing.QrCode;

namespace DigitalFUHubApi.Services
{
	public class TwoFactorAuthenticationService
	{
		private readonly IConfiguration configuration;

		public TwoFactorAuthenticationService(IConfiguration _configuration)
		{
			configuration = _configuration;	
		}

		internal string GenerateSecretKey()
		{
			return Guid.NewGuid().ToString();
		}

		internal string GenerateQrCode(string secretKey, string userName)
		{
			var totp = new TwoFactorAuthenticator();
			var setupInfo = totp.GenerateSetupCode(configuration["JWT:Issuer"], userName, secretKey,false,3);
			var qrCode = setupInfo.QrCodeSetupImageUrl;
			return qrCode;
		}

		internal bool ValidateTwoFactorPin(string secretKey, string otp)
		{
			var totp = new TwoFactorAuthenticator();
			return totp.ValidateTwoFactorPIN(secretKey, otp);
		}
	}
}
