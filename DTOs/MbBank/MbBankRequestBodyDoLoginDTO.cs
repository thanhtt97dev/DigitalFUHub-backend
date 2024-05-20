using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.MbBank
{
	public class MbBankRequestBodyDoLoginDTO
	{
		public string? captcha { get; set; }
		public string? deviceIdCommon { get; set; }
		public string? ibAuthen2faString { get; set; }
		public string? password { get; set; }
		public string? refNo { get; set; }
		public string? sessionId { get; set; }
		public string? userId { get; set; }
	}
}
