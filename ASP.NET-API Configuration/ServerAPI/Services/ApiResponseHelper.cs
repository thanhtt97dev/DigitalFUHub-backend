using Microsoft.AspNetCore.Mvc;

namespace ServerAPI.Services
{
	public class ApiResponseHelper
	{
		public  IActionResult StatusCode(int statusCode, string? message = null)
		{
			var response = new
			{
				message = message ?? GetDefaultMessageForStatusCode(statusCode),
				code = statusCode
			};

			return new ObjectResult(response)
			{
				StatusCode = statusCode
			};
		}

		private string GetDefaultMessageForStatusCode(int statusCode)
		{
			switch (statusCode)
			{
				case 400:
					return "Bad Request";
				case 401:
					return "Unauthorized";
				case 403:
					return "Forbidden";
				case 404:
					return "Not Found";
				case 500:
					return "Internal Server Error";
				default:
					return "Unknown Status Code";
			}
		}
	}
}

