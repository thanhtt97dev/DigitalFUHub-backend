using AspNetCoreRateLimit;

namespace ServerAPI.Comons
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
	}
}
