using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Feedback
{
	public class CustomerFeedbackDetailOrderRequestDTO
	{
        public long UserId { get; set; }
        public long OrderId { get; set; }
    }
	public class CustomerFeedbackDetailOrderResponseDTO
	{
		public string ProductName { get; set; } = string.Empty!;
		public string Thumbnail { get; set; } = string.Empty!;
		public string ProductVariantName { get; set; } = string.Empty!;
		public int Rate { get; set; }
		public string Content { get; set; } = string.Empty!;
		public List<string> urlImages { get; set; } = null!;
	}
}
