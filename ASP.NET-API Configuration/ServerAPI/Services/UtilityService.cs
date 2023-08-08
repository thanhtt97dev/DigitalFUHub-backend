using ServerAPI.Services;

namespace ServerAPI.Services
{
	public class UtilityService
	{

		private static UtilityService? instance;
		private static readonly object instanceLock = new object();

		public static UtilityService Instance
		{
			get
			{
				lock (instanceLock)
				{
					if (instance == null)
					{
						instance = new UtilityService();
					}
				}
				return instance;
			}
		}

		public DateTime ConvertUnitTimeToDateTime(long? utcExpireDate)
		{
			var dateTimeInterval = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			dateTimeInterval = dateTimeInterval.AddSeconds(utcExpireDate ?? 0).ToUniversalTime();
			return dateTimeInterval;
		}
	}
}
