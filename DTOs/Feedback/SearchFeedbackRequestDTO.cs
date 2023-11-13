using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Feedback
{
	public class SearchFeedbackRequestDTO
	{
		public long ProductId { get; set; }	
		public int Type { get; set; }	
		public int Page { get; set; }
	}
}
