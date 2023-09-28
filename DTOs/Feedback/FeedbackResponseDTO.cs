using BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs.Seller;
using DTOs.User;

namespace DTOs.Feedback
{
    public class FeedbackResponseDTO
    {
        public long FeedbackId { get; set; }
        public string? Content { get; set; }
        public int Rate { get; set; }
        public DateTime UpdateAt { get; set; }
        public UserResponeDTO User { get; set; } = null!;
        public ICollection<FeedbackMediaResponseDTO>? FeedbackMedias { get; set; }
    }
}
