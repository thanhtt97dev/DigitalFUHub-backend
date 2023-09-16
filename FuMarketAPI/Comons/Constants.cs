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
		public static string BankIdMbBank = "970422";
	}
}
