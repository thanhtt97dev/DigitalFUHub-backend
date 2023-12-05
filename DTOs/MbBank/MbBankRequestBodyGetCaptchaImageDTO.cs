using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.MbBank
{
	public class MbBankRequestBodyGetCaptchaImageDTO
	{
		public string? deviceIdCommon { get; set; }
		public string? refNo { get; set; }
		public string? sessionId { get; set; }
	}
}
