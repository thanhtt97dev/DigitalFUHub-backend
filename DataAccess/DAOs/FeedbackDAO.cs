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
using Comons;
using System.Linq.Expressions;

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
						UpdateAt = feedback.DateUpdate,
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

		internal void AddFeedbackOrder(long userId, long orderId, long orderDetailId, string content, int rate, List<string> urlImages)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var transaction = context.Database.BeginTransaction();
				try
				{
					Order? order = context.Order.Include(x => x.OrderDetails).ThenInclude(x => x.ProductVariant)
						.FirstOrDefault(x => x.UserId == userId && x.OrderId == orderId
						&& x.OrderDetails.Any(od => od.OrderDetailId == orderDetailId));
					if (order == null) throw new Exception("NOT FOUND.");
					if (DateTime.Now.Subtract(order.OrderDate) > TimeSpan.FromDays(Constants.NUMBER_DAYS_CAN_MAKE_FEEDBACK)) throw new Exception("EXCEED TIME TO FEEDBACK.");

					User user = context.User.First(x => x.UserId == userId);

					OrderDetail orderDetail = order.OrderDetails.First(x => x.OrderDetailId == orderDetailId);
					if (orderDetail.IsFeedback) throw new Exception("NOT FEEDBACK AGAIN.");
					Product product = context.Product.First(x => x.ProductId == orderDetail.ProductVariant.ProductId);

					//update product
					product.TotalRatingStar += rate;
					product.NumberFeedback += 1;
					context.Product.Update(product);
					context.SaveChanges();

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
						DateCreate = DateTime.Now,
						DateUpdate = DateTime.Now,
					};
					if (urlImages.Count > 0)
					{
						feedback.FeedbackMedias = urlImages.Select(x => new FeedbackMedia
						{
							Url = x,
						}).ToList();
					}
					context.Feedback.Add(feedback);
					context.SaveChanges();

					if (feedbackBenefit.Coin > 0)
					{
						//update user 
						user.Coin += feedbackBenefit.Coin;
						//update orderDetail
						orderDetail.IsFeedback = true;

						//Add new transaction coin
						TransactionCoin transactionCoin = new TransactionCoin
						{
							OrderId = orderDetail.OrderId,
							UserId = user.UserId,
							TransactionCoinTypeId = Constants.TRANSACTION_COIN_TYPE_RECEIVE,
							FeedbackId = feedback.FeedbackId,
							Amount = feedbackBenefit.Coin,
							DateCreate = DateTime.Now
						};

						context.TransactionCoin.Add(transactionCoin);
						context.SaveChanges();
					}
					context.SaveChanges();
					transaction.Commit();
				}
				catch (Exception e)
				{
					transaction.Rollback();
					throw new Exception(e.Message);
				}
			}
		}

		internal Order? GetFeedbackDetail(long orderId, long userId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				Order? order = context.Order
					.Include(x => x.User)
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

		internal (long, List<Order>) GetListFeedbackSeller(long userId, string orderId, string userName, string productName,
			string productVariantName, DateTime? fromDate, int rate, int page)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var query = (context.Order
					.Include(x => x.User)
					.Include(x => x.OrderDetails)
					.ThenInclude(x => x.ProductVariant)
					.ThenInclude(x => x.Product)
					.Include(x => x.OrderDetails)
					.ThenInclude(x => x.Feedback)
					.ThenInclude(x => x.FeedbackMedias)
					.Where(x => x.ShopId == userId
							&& x.User.Username.ToLower().Contains(userName.ToLower())
							&& (string.IsNullOrWhiteSpace(orderId) ? true : x.OrderId.ToString().Trim().ToLower() == orderId))
					.Select(x => new Order
					{
						OrderId = x.OrderId,
						OrderDate = x.OrderDate,
						User = new User
						{
							Username = x.User.Username,
							Avatar = x.User.Avatar,
						},
						OrderDetails = x.OrderDetails.Where(od => od.IsFeedback == true
							&& (rate == 0 ? true : od.Feedback.Rate == rate)
							&& (fromDate == null ? true : fromDate.Value.Date <= od.Feedback.DateUpdate.Date)
							&& (od.ProductVariant.Product.ProductName ?? "").ToLower().Contains(productName.ToLower())
							&& (od.ProductVariant.Name ?? "").ToLower().Contains(productVariantName.ToLower()))
							.Select(od => new OrderDetail
							{
								OrderId = od.OrderId,
								Feedback = new Feedback
								{
									Content = od.Feedback.Content ?? "",
									Rate = od.Feedback.Rate,
									DateUpdate = od.Feedback.DateUpdate,
									FeedbackMedias = od.Feedback.FeedbackMedias,
								},
								ProductVariant = od.ProductVariant,
							}).ToList(),
					})
					.ToList())
					.Where(x => x.OrderDetails.Count > 0);

				return (query.Count(), query.Skip((page - 1) * Constants.PAGE_SIZE_FEEDBACK).Take(Constants.PAGE_SIZE_FEEDBACK).ToList());

			}
		}

		internal int GetNumberFeedbackWithCondition(long productId, int type, int page)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var result = (from feedback in context.Feedback
							  where
								  feedback.ProductId == productId &&
								  (
									(type == Constants.FEEDBACK_TYPE_ALL) ||
									  (
										  ((type == Constants.FEEDBACK_TYPE_1_STAR) ? feedback.Rate == 1 : true) &&
										  ((type == Constants.FEEDBACK_TYPE_2_STAR) ? feedback.Rate == 2 : true) &&
										  ((type == Constants.FEEDBACK_TYPE_3_STAR) ? feedback.Rate == 3 : true) &&
										  ((type == Constants.FEEDBACK_TYPE_4_STAR) ? feedback.Rate == 4 : true) &&
										  ((type == Constants.FEEDBACK_TYPE_5_STAR) ? feedback.Rate == 5 : true) &&
										  ((type == Constants.FEEDBACK_TYPE_HAVE_COMMENT) ? !string.IsNullOrEmpty(feedback.Content) : true) &&
										  ((type == Constants.FEEDBACK_TYPE_HAVE_MEDIA) ?
											  (from feedbackMedia in context.FeedbackMedia
											   where feedbackMedia.FeedbackId == feedback.FeedbackId
											   select new { }
											  ).Count() > 0
											  :
											  true
										  )
									  )
									)
							  select new { }
								).Count();

				return result;
			}
		}

		internal List<Feedback> GetFeedbacksWithCondition(long productId, int type, int page)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var feedbacks = (from feedback in context.Feedback
								 join user in context.User
									on feedback.UserId equals user.UserId
								 join orderDetail in context.OrderDetail
									on feedback.OrderDetailId equals orderDetail.OrderDetailId
								 join productVariant in context.ProductVariant
									on orderDetail.ProductVariantId equals productVariant.ProductVariantId
								 where
									 feedback.ProductId == productId &&
									 (
										 (type == Constants.FEEDBACK_TYPE_ALL) ||
										 (
											 ((type == Constants.FEEDBACK_TYPE_1_STAR) ? feedback.Rate == 1 : true) &&
											 ((type == Constants.FEEDBACK_TYPE_2_STAR) ? feedback.Rate == 2 : true) &&
											 ((type == Constants.FEEDBACK_TYPE_3_STAR) ? feedback.Rate == 3 : true) &&
											 ((type == Constants.FEEDBACK_TYPE_4_STAR) ? feedback.Rate == 4 : true) &&
											 ((type == Constants.FEEDBACK_TYPE_5_STAR) ? feedback.Rate == 5 : true) &&
											 ((type == Constants.FEEDBACK_TYPE_HAVE_COMMENT) ? !string.IsNullOrEmpty(feedback.Content) : true) &&
											 ((type == Constants.FEEDBACK_TYPE_HAVE_MEDIA) ?
												 (from feedbackMedia in context.FeedbackMedia
												  where feedbackMedia.FeedbackId == feedback.FeedbackId
												  select new { }
												 ).Count() > 0
												 :
												 true
											 )
										 )
									 )
								 select new Feedback
								 {
									 FeedbackId = feedback.FeedbackId,
									 User = new User
									 {
										 UserId = user.UserId,
										 Fullname = user.Fullname,
										 Avatar = user.Avatar
									 },
									 OrderDetail = new OrderDetail
									 {
										 ProductVariant = new ProductVariant
										 {
											 Name = productVariant.Name,
										 }
									 },
									 Content = feedback.Content,
									 Rate = feedback.Rate,
									 DateUpdate = feedback.DateUpdate,
									 FeedbackMedias = context.FeedbackMedia.Where(x => x.FeedbackId == feedback.FeedbackId).ToList(),	
								 }
								)
								.OrderBy(x => x.DateUpdate)
								.ToList();
				return feedbacks;
			}
		}
	}
}
