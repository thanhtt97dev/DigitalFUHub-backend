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

		public long AddFeedbackOrder(long userId, long orderId, long orderDetailId, string content, int rate, List<string> urlImages)
		=> FeedbackDAO.Instance.AddFeedbackOrder(userId, orderId, orderDetailId, content, rate, urlImages);

		public List<FeedbackResponseDTO> GetFeedbacks(long productId)
		{
			if (productId == 0) throw new ArgumentException("ProductId invalid (at GetFeedbacks)");
			return FeedbackDAO.Instance.GetFeedbacks(productId);
		}

		public (long, List<Order>) GetListFeedbackSeller(long userId, string orderId, string userName, string productName,
			string productVariantName, DateTime? fromDate, DateTime? toDate, int rate,int page)
		=> FeedbackDAO.Instance.GetListFeedbackSeller(userId, orderId, userName, productName, productVariantName, fromDate, toDate, rate, page);

		public int GetNumberFeedbackWithCondition(long productId, int type, int page) => FeedbackDAO.Instance.GetNumberFeedbackWithCondition(productId, type, page);

		public List<Feedback> GetFeedbacksWithCondition(long productId, int type, int page) => FeedbackDAO.Instance.GetFeedbacksWithCondition(productId, type, page);

		public Order? GetFeedbackDetailOrderOfSeller(long orderId, long userId)
		=> FeedbackDAO.Instance.GetFeedbackDetailOrderOfSeller(orderId, userId);
	}
}
