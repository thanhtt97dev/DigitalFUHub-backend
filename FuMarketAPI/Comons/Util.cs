using System.Text;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace FuMarketAPI.Comons
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

		#region Read file
		public static string ReadFile(string fileName)
		{
			string fullPath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
			string text = File.ReadAllText(fullPath);
			return text;
		}
		#endregion

		#region Write file
		public static void WriteFile(string fileName, object data)

		{
			string fullPath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
			var options = new JsonSerializerOptions { WriteIndented = true };
			string json = JsonSerializer.Serialize(data, options);
			File.WriteAllText(fullPath, json);
		}
		#endregion

		#region Get random string
		public static string GetRandomString(int length)
		{
			StringBuilder str_build = new StringBuilder();
			Random random = new Random();

			char letter;

			for (int i = 0; i < length; i++)
			{
				double flt = random.NextDouble();
				int shift = Convert.ToInt32(Math.Floor(25 * flt));
				letter = Convert.ToChar(shift + 65);
				str_build.Append(letter);
			}
			return str_build.ToString();
		}
		#endregion

		#region Hide text with * charactor
		public static string HideCharacters(string input, int startAt)
		{
			if (input.Length <= startAt)
			{
				return new string('*', input.Length);
			}
			else
			{
				string hiddenText = new string('*', startAt) + input.Substring(startAt);
				return hiddenText;
			}
		}
		#endregion

		#region Compare Date Equal Greater Than Days Condition
		public static bool CompareDateEqualGreaterThanDaysCondition(DateTime start, int days)
		{
			if (days < 0 ) return false;
			DateTime end = start.AddDays(days);
			DateTime today = DateTime.Now;
			if(end >= today) return false;
			return true;
		}
		#endregion
	}
}
