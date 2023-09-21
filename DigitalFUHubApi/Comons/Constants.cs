using AspNetCoreRateLimit;

namespace DigitalFUHubApi.Comons
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

		public static string MB_BANK_RESPONE_CODE_SUCCESS = "00";
		public static string MB_BANK_RESPONE_CODE_SESSION_INVALID = "GW200";
		public static string MB_BANK_RESPONE_CODE_SAME_URI_IN_SAME_TIME = "GW485";
		public static string MB_BANK_RESPONE_CODE_ACCOUNT_NOT_FOUND = "MC201";
		public static string MB_BANK_RESPONE_CODE_SEARCH_WITH_TYPE_ERR = "MC231";

		//User config
		public static int ADMIN_ROLE = 1;

		//Respone code
		public static string RESPONSE_CODE_SUCCESS = "00";
		public static string RESPONSE_CODE_NOT_ACCEPT = "01";
		public static string RESPONSE_CODE_DATA_NOT_FOUND = "02";
		public static string RESPONSE_CODE_FAILD = "03";
		
	}
}
