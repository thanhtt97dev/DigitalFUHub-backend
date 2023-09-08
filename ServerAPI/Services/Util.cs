using ServerAPI.Services;

namespace ServerAPI.Services
{
	public class Util
	{

		private static Util? instance;
		private static readonly object instanceLock = new object();

		public static Util Instance
		{
			get
			{
				lock (instanceLock)
				{
					if (instance == null)
					{
						instance = new Util();
					}
				}
				return instance;
			}
		}

		#region Convert unit date to DateTime
		public DateTime ConvertUnitTimeToDateTime(long? utcExpireDate)
		{
			var dateTimeInterval = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			dateTimeInterval = dateTimeInterval.AddSeconds(utcExpireDate ?? 0).ToUniversalTime();
			return dateTimeInterval;
		}
		#endregion

		#region Get access token from httpContext
		public string GetAccessToken(HttpContext httpContext)
		{
			string token = string.Empty;
			string authorization = httpContext.Request.Headers["Authorization"].ToString();
			string tokenType = "Bearer ";
			if (authorization.Contains(tokenType))
			{
				token = authorization.Substring(tokenType.Length);
			}
			return token;	
		}
		#endregion
	}
}
