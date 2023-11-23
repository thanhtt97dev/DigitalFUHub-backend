using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Admin
{
	public class BusinessFeeRequestDTO
	{
		public string? BusinessFeeId { get; set; }
		public int MaxFee { get; set; }
		public string? FromDate { get; set; } = null!;
		public string? ToDate { get; set; } = null!;
	}
}
