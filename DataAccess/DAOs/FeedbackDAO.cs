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
                        User = new UserResponeDTO {
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


    }
}
