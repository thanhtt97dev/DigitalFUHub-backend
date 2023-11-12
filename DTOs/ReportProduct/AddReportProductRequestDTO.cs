using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.ReportProduct
{
    public class AddReportProductRequestDTO
    {
        public long UserId { get; set; }
        public long ProductId { get; set; }
        public int ReasonReportProductId { get; set; }
        public string Description { get; set; } = string.Empty!;
    }
}
