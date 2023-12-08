using BusinessObject.Entities;
using DTOs.Admin;
using DTOs.FeedbackBenefit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.IRepositories
{
    public interface IFeedbackBenefitRepository
    {
        List<FeedbackBenefitAdminResponseDTO> GetFeedbackBenefits(long feedbackBenefitId, long maxCoin, DateTime? fromDate, DateTime? toDate);
        void AddNewFeedbackBenefit(long coin);
    }
}
