using Azure.Core;
using BusinessObject.Entities;
using Comons;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

namespace DigitalFUHubApi.Comons
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
		public static string GetAccessToken(HttpContext httpContext)
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

		#region Get file
		public static byte[]? GetFile(string fileName)
		{
			string fullPath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
			try
			{
				return File.ReadAllBytes(fullPath);
			}
			catch (Exception)
			{
				return null;
			}
			
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

		#region Write file
		public static void WriteFileFullPath(string fullPath, object data)
		{
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
			if (days < 0) return false;
			DateTime end = start.AddDays(days);
			DateTime today = DateTime.Now;
			if (end >= today) return false;
			return true;
		}
		#endregion

		#region Get Content Type File
		public string GetContentType(string filename)
		{
			string contentType;
			string fileExtension = filename.Substring(filename.LastIndexOf("."));
			if (fileExtension.Contains(".jpg") || fileExtension.Contains(".jpeg"))
			{
				contentType = "image/jpeg";
			}
			else if (fileExtension.Contains(".png"))
			{
				contentType = "image/png";
			}
			//else if (fileExtension.Contains(".gif"))
			//{
			//	contentType = "image/gif";
			//}
			//else if (fileExtension.Contains(".txt"))
			//{
			//	contentType = "text/xml";
			//}
			//else if (fileExtension.Contains(".mp3") || fileExtension.Contains(".mp4"))
			//{
			//	contentType = "audio/mpeg";
			//}
			else
			{
				contentType = "application/octet-stream";
			}
			return contentType;
		}
		#endregion

		#region Sha256Hash
		public string Sha256Hash(string input)
		{
			SHA256 sha256 = SHA256.Create();
			byte[] sha256Bytes = Encoding.Default.GetBytes(input);
			byte[] cryString = sha256.ComputeHash(sha256Bytes);
			string sha256Str = Convert.ToHexString(cryString).ToLower();
			return sha256Str;
		}
		#endregion

		#region Generate random password
		public string RandomPassword8Chars()
		{
			string password = string.Empty;
			string numbers = "0123456789";
			string letters = "qwertyuiopasdfghjklzxcvbnm";
			Random rnd = new Random();
			for (int i = 0; i < 8; i++)
			{
				int rndType = rnd.Next(1, 4);
				if (rndType == 1)
				{
					int rndIndex = rnd.Next(0, numbers.Length);
					password += numbers[rndIndex];
				}
				else if (rndType == 2)
				{
					int rndIndex = rnd.Next(0, letters.Length);
					password += letters[rndIndex];
				}
				else if (rndType == 3)
				{
					int rndIndex = rnd.Next(0, letters.Length);
					password += (char)(letters[rndIndex] - 32);
				}
			}
			return password;
		}
		#endregion

		#region Read Data File Excel Product Variant
		public List<AssetInformation> ReadDataFileExcelProductVariant(IFormFile file)
		{
			List<AssetInformation> result = new List<AssetInformation>();
			using (ExcelPackage excelPackage = new ExcelPackage(file.OpenReadStream()))
			{
				foreach (ExcelWorksheet worksheet in excelPackage.Workbook.Worksheets)
				{
					for (int i = worksheet.Dimension.Start.Row; i <= worksheet.Dimension.End.Row; i++)
					{
						if (worksheet.Cells[i, 1].Value != null && i != 1)
						{
							string value = worksheet.Cells[i, 1].Value?.ToString()?.Trim() ?? "";
							if (!string.IsNullOrEmpty(value))
							{
								result.Add(new AssetInformation
								{
									CreateDate = DateTime.Now,
									Asset = value,
									IsActive = true,
								});
							}
						}
					}
					break;
				}
			}
			return result;
		}
		#endregion

		#region Get from date - to date
		public static (bool, DateTime?, DateTime?) GetFromDateToDate(string? from, string? to)
		{
			DateTime? fromDate = null;
			DateTime? toDate = null;
			string format = "M/d/yyyy";
			if (!string.IsNullOrEmpty(from) && !string.IsNullOrEmpty(to))
			{
				try
				{
					fromDate = DateTime.ParseExact(from, format, System.Globalization.CultureInfo.InvariantCulture);
					toDate = DateTime.ParseExact(to, format, System.Globalization.CultureInfo.InvariantCulture).AddDays(1);
					if (fromDate > toDate)
					{
						return (false, null, null);
					}
				}
				catch (FormatException)
				{
					return (false, null, null);
				}

			}
			return (true, fromDate, toDate);
		}
        #endregion

        #region Check Url Valid
        public static bool IsUrlValid(string url)
        {
            Uri? uriResult;
            bool isValidUrl = Uri.TryCreate(url, UriKind.Absolute, out uriResult)
                              && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            return isValidUrl;
        }
		#endregion

		#region Write log
		public static void WirteToLogFile(string message)
		{
			var currentDate = DateTime.Now.ToString("yyyyMMdd");
			var currentTime = DateTime.Now.ToString("yyyyMMddHHmmss");
			var folderName = "Log";
			var folderLog = Path.Combine(Directory.GetCurrentDirectory(), folderName);
			if(!Directory.Exists(folderLog)) 
			{
				Directory.CreateDirectory(folderLog);
			}
			var folderLogToday = Path.Combine(Directory.GetCurrentDirectory(), currentDate);
			if (!Directory.Exists(folderLogToday))
			{
				Directory.CreateDirectory(folderLogToday);
			}
			var data = $"[{currentTime}] \r --- {message} \r\r\r";

			WriteFileFullPath(folderLogToday, data);
		}
		#endregion
	}
}
