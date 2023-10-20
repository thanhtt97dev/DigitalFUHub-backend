using BusinessObject.Entities;
using DataAccess.DAOs;
using DataAccess.IRepositories;
using DTOs.Feedback;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
	public class FeedbackRepository : IFeedbackRepository
	{
		public Order? GetFeedbackDetail(long orderId, long userId)
		=> FeedbackDAO.Instance.GetFeedbackDetail(orderId, userId);

		public void AddFeedbackOrder(long userId, long orderId, long orderDetailId, string content, int rate, List<string> urlImages)
		=> FeedbackDAO.Instance.AddFeedbackOrder(userId, orderId, orderDetailId, content, rate, urlImages);

		public List<FeedbackResponseDTO> GetFeedbacks(long productId)
		{
			if (productId == 0) throw new ArgumentException("ProductId invalid (at GetFeedbacks)");
			return FeedbackDAO.Instance.GetFeedbacks(productId);
		}
	}
}
