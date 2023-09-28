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
        public List<FeedbackResponseDTO> GetFeedbacks(long productId)
        {
            if (productId == 0) throw new ArgumentException("ProductId invalid (at GetFeedbacks)");
            return FeedbackDAO.Instance.GetFeedbacks(productId);
        }
    }
}
