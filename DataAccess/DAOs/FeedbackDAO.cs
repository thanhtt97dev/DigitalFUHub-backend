using BusinessObject.Entities;
using BusinessObject;
using DTOs.Product;
using DTOs.Seller;
using DTOs.Tag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs.Feedback;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using DTOs.User;
using System.Security.Cryptography.X509Certificates;

namespace DataAccess.DAOs
{
	public class FeedbackDAO
	{

		private static FeedbackDAO? instance;
		private static readonly object instanceLock = new object();

		public static FeedbackDAO Instance
		{
			get
			{
				lock (instanceLock)
				{
					if (instance == null)
					{
						instance = new FeedbackDAO();
					}
				}
				return instance;
			}
		}


		internal List<FeedbackResponseDTO> GetFeedbacks(long productId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				List<FeedbackResponseDTO> feedbackResponses = new List<FeedbackResponseDTO>();
				var feedbacks = context.Feedback.Include(u => u.User).Where(f => f.ProductId == productId).ToList();
				if (feedbacks == null || feedbacks.Count == 0)
				{
					return feedbackResponses;
				}

				List<FeedbackMediaResponseDTO> feedbackMediaResponses;

				foreach (var feedback in feedbacks)
				{
					feedbackMediaResponses = context.FeedbackMedia.Where(f => f.FeedbackId == feedback.FeedbackId).
						Select(f =>
							new FeedbackMediaResponseDTO
							{
								FeedbackMediaId = f.FeedbackMediaId,
								FeedbackId = f.FeedbackId,
								Url = f.Url
							}
						).ToList();

					feedbackResponses.Add(new FeedbackResponseDTO()
					{
						FeedbackId = feedback.FeedbackId,
						Content = feedback.Content,
						Rate = feedback.Rate,
						UpdateAt = feedback.UpdateDate,
						User = new UserResponeDTO
						{
							UserId = feedback.User.UserId,
							RoleId = feedback.User.RoleId,
							Email = feedback.User.Email,
							Avatar = feedback.User.Avatar,
						},
						FeedbackMedias = feedbackMediaResponses
					});
				}

				return feedbackResponses;
			}
		}

		internal void FeedbackOrder(long userId, long orderId, long orderDetailId, string content, int rate, List<string> urlImages)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				try
				{
					Order? order = context.Order.Include(x => x.OrderDetails).ThenInclude(x => x.ProductVariant)
						.FirstOrDefault(x => x.UserId == userId && x.OrderId == orderId
						&& x.OrderDetails.Any(od => od.OrderDetailId == orderDetailId));
					if (order == null) throw new Exception("Order not found!");

					User user = context.User.First(x => x.UserId == userId);

					OrderDetail orderDetail = order.OrderDetails.First(x => x.OrderDetailId == orderDetailId);

					Product product = context.Product.First(x => x.ProductId == orderDetail.ProductVariant.ProductId);

					FeedbackBenefit feedbackBenefit = context.FeedbackBenefit
						.OrderByDescending(x => x.FeedbackBenefitId)
						.First(x => x.EndDate == null);

					Feedback feedback = new Feedback
					{
						OrderDetailId = orderDetailId,
						Content = content,
						Rate = rate,
						FeedbackBenefitId = feedbackBenefit.FeedbackBenefitId,
						ProductId = product.ProductId,
						UserId = userId,
						UpdateDate = DateTime.Now,
					};
					if(urlImages.Count > 0)
					{
						feedback.FeedbackMedias = urlImages.Select(x => new FeedbackMedia
						{
							Url = x,
						}).ToList();
					}
					user.Coin += feedbackBenefit.Coin;
					orderDetail.IsFeedback = true;
					context.Feedback.Add(feedback);
					context.SaveChanges();
				}
				catch (Exception e)
				{
					throw new Exception(e.Message);
				}
			}
		}

		internal Order? FeedbackOrder(long orderId, long userId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				Order? order = context.Order
					.Include(x => x.OrderDetails)
					.ThenInclude(x => x.Feedback)
					.ThenInclude(x => x.FeedbackMedias)
					.Include(x => x.OrderDetails)
					.ThenInclude(x => x.ProductVariant)
					.ThenInclude(x => x.Product)
					.FirstOrDefault(x => x.UserId == userId && x.OrderId == orderId);
				return order;

			}
		}
	}
}
