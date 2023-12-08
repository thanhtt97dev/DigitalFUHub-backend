using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
	public class FeedbackBenefit
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long FeedbackBenefitId { get; set; }
		public long Coin { get; set; }	
		public DateTime StartDate { get; set; }	
		public DateTime? EndDate { get; set; }
	}
}
