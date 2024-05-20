using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.MbBank
{
	public class MbBankAccount
	{
		public string AccountNo { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
		public string DeviceIdCommon { get; set; } = string.Empty;
		public string RefNo { get; set; } = string.Empty;
		public string IbAuthen2faString { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
	}
}
