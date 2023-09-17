using AspNetCoreRateLimit;

namespace FuMarketAPI.Comons
{
	public static class Constants
	{
		public static List<RateLimitRule> RateLimitRules = new List<RateLimitRule>()
		{
			new RateLimitRule
			{
				Endpoint = "*",
				Period = "10s",
				Limit = 10,
			}
		};
		public static string BANK_ID_MB_BANK = "970422";
		public static int NUMBER_DAYS_CAN_UPDATE_BANK_ACCOUNT = 15;
	}
}
