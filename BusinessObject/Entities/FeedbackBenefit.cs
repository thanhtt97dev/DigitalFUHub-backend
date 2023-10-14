using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
	public class FeedbackBenefit
	{
		[Key]
		public int FeedbackBenefitId { get; set; }
		public int Coin { get; set; }	
		public DateTime StartDate { get; set; }	
		public DateTime? EndDate { get; set; }
	}
}
