using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.ReasonReportProduct
{
    public class ReasonReportProductResponseDTO
    {
        public int ReasonReportProductId { get; set; }
        public string ViName { get; set; } = string.Empty;
        public string ViExplanation { get; set; } = string.Empty;
    }
}
