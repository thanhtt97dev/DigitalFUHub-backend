using BusinessObject.Entities;
using DTOs.Feedback;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.IRepositories
{
    public interface IFeedbackRepository
    {
		Order? FeedbackDetail(long orderId, long userId);
		void FeedbackOrder(long userId, long orderId, long orderDetailId, string content, int rate, List<string> urlImages);
		List<FeedbackResponseDTO> GetFeedbacks(long productId);
    }
}
