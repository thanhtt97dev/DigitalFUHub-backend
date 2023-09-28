using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Feedback
{
    public class FeedbackMediaResponseDTO
    {
        public long FeedbackMediaId { get; set; }
        public long FeedbackId { get; set; }
        public string Url { get; set; } = null!;
    }
}
