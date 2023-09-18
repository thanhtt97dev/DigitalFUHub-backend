using AspNetCoreRateLimit;

namespace FuMarketAPI.Comons
{
	public static class Constants
	{
		//program.cs config
		public static List<RateLimitRule> RateLimitRules = new List<RateLimitRule>()
		{
			new RateLimitRule
			{
				Endpoint = "*",
				Period = "10s",
				Limit = 10,
			}
		};

		//MB bank config
		public static string BANK_ID_MB_BANK = "970422";
		public static int NUMBER_DAYS_CAN_UPDATE_BANK_ACCOUNT = 15;

		//User config
		public static int ADMIN_ROLE = 1;
	}
}
