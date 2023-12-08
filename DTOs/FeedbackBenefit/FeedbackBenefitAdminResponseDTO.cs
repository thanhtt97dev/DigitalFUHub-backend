using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.FeedbackBenefit
{
    public class FeedbackBenefitAdminResponseDTO
    {
        public long FeedbackBenefitId { get; set; }
        public long Coin { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int TotalFeedbackUsed { get; set; }
    }
}
