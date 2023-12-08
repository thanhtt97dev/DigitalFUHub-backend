using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.BusinessFee
{
    public class FeedbackBenefitAdminRequestDTO
    {
        public string? FeedbackBenefitId { get; set; }
        public long MaxCoin { get; set; }
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
    }
}
