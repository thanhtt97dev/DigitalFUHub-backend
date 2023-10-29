using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Feedback
{
	public class CustomerFeedbackDetailOrderResponseDTO
	{
		public string Username { get; set; } = string.Empty!;
		public string Avatar { get; set; } = string.Empty!;
		public string ProductName { get; set; } = string.Empty!;
		public string Thumbnail { get; set; } = string.Empty!;
		public string ProductVariantName { get; set; } = string.Empty!;
		public int Quantity { get; set; }
		public int Rate { get; set; }
		public string Content { get; set; } = string.Empty!;
		public DateTime Date { get; set; }
		public List<string> UrlImages { get; set; } = null!;
	}
}
