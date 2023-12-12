using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Seller
{
	public class SellerFeedbackRequestDTO
	{
		public long UserId { get; set; }
		public string OrderId { get; set; } = string.Empty;
		public string ProductName { get; set; } = string.Empty;
		public string ProductVariantName { get; set; } = string.Empty;
		public string? FromDate { get; set; } = null!;
		public string? ToDate { get; set; } = null!;
		public string UserName { get; set; } = string.Empty;
		public int Rate { get; set; }
		public int Page { get; set; }
	}
	public class ListFeedbackResponseDTO
	{
		public long TotalItems { get; set; }
		public List<SellerFeedbackResponseDTO> Feedbacks { get; set; } = new();
	} 
	public class SellerFeedbackResponseDTO
	{
		public long OrderId { get; set; }
		public DateTime OrderDate { get; set; }
		public string CustomerUsername { get; set; } = string.Empty;
		public string CustomerAvatar { get; set; } = string.Empty;
		public List<SellerFeedbackDetailResponseDTO> Detail { get; set; } = null!;
	}
	public class SellerFeedbackDetailResponseDTO
	{
		public long ProductId{ get; set; }
		public string ProductName { get; set; } = string.Empty;
		public string Thumbnail { get; set; } = string.Empty;
		public string ProductVariantName { get; set; } = string.Empty;
		public string Content { get; set; } = string.Empty;
		public int Rate { get; set; }
		public DateTime FeedbackDate { get; set; } 
		public List<string> UrlImageFeedback { get; set; } = null!;

	}
}
