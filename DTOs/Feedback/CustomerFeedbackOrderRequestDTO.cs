using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Feedback
{
	public class CustomerFeedbackOrderRequestDTO
	{
		public long UserId { get; set; }
		public long OrderId { get; set; }
		public long OrderDetailId { get; set; }
		public int Rate { get; set; }
		public string? Content { get; set; } = null!;
		public List<IFormFile>? ImageFiles { get; set; } = null!;
	}
}
