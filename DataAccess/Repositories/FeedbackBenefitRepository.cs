using BusinessObject.Entities;
using DataAccess.DAOs;
using DataAccess.IRepositories;
using DTOs.FeedbackBenefit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DataAccess.Repositories
{
    public class FeedbackBenefitRepository : IFeedbackBenefitRepository
    {
        public void AddNewFeedbackBenefit(long coin) => FeedbackBenefitDAO.Instance.AddNewFeedbackBenefit(coin);

        public List<FeedbackBenefitAdminResponseDTO> GetFeedbackBenefits(long feedbackBenefitId, long coin, DateTime? fromDate, DateTime? toDate)
            => FeedbackBenefitDAO.Instance.GetFeedbackBenefits(feedbackBenefitId, coin, fromDate, toDate);
    }
}
