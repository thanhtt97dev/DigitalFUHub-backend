using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Product
{
	public class ProductDetailFeedBackGeneralInfo
	{
		public long TotalRatingStar { get; set; }
		public long TotalFeedback { get; set; }
		public long TotalFeedbackFiveStar { get; set; }
		public long TotalFeedbackFourStar { get; set; }
		public long TotalFeedbackThreeStar { get; set; }
		public long TotalFeedbackTwoStar { get; set; }
		public long TotalFeedbackOneStar { get; set; }
		public long TotalFeedbackHaveComment { get; set; }
		public long TotalFeedbackHaveMedia { get; set; }
	}
}
