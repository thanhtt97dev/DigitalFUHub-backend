using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Feedback
{
	public class SearchFeedbackResponseDTO
	{
		public int TotalFeedback {get; set;}
		public List<SearchFeedbackDetailResponseDTO>? Feedbacks {  get; set; }	
	}

	public class SearchFeedbackDetailResponseDTO
	{
		public long FeedbackId { get; set; }
		public long UserId { get; set; }
		public string UserName { get; set; } = string.Empty;
		public string UserAvatar { get; set; } = string.Empty;
		public string ProductVariantName { get; set; } = string.Empty;
		public string? Content { get; set; }
		public int Rate { get; set; }
		public List<string> FeedbackMedias { get; set; } = new List<string>();
		public DateTime DateUpdate { get; set; }
	}
}
