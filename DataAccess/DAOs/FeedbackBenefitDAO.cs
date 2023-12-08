using BusinessObject.Entities;
using BusinessObject;
using DTOs.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs.FeedbackBenefit;

namespace DataAccess.DAOs
{
    internal class FeedbackBenefitDAO
    {
        private static FeedbackBenefitDAO? instance;
        private static readonly object instanceLock = new object();

        public static FeedbackBenefitDAO Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new FeedbackBenefitDAO();
                    }
                }
                return instance;
            }
        }

        internal List<FeedbackBenefitAdminResponseDTO> GetFeedbackBenefits(long feedbackBenefitId, long maxCoin, DateTime? fromDate, DateTime? toDate)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                var feedbackBenefits = (from feebackBenefit in context.FeedbackBenefit
                            where (1 == 1) &&
                            (feedbackBenefitId != 0 ? feebackBenefit.FeedbackBenefitId == feedbackBenefitId : true) &&
                            (fromDate != null && toDate != null) ? fromDate <= feebackBenefit.StartDate && toDate >= feebackBenefit.StartDate : true &&
                            feebackBenefit.Coin <= maxCoin
                            select new FeedbackBenefitAdminResponseDTO
                            {
                                FeedbackBenefitId = feebackBenefit.FeedbackBenefitId,
                                Coin = feebackBenefit.Coin,
                                StartDate = feebackBenefit.StartDate,
                                EndDate = feebackBenefit.EndDate,
                                TotalFeedbackUsed = context.Feedback.Count(x => x.FeedbackBenefitId == feebackBenefit.FeedbackBenefitId)
                            })
                            .OrderByDescending(x => x.FeedbackBenefitId)
                            .ToList();

                return feedbackBenefits;
            }
        }

        internal void AddNewFeedbackBenefit (long coin)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                var transaction = context.Database.BeginTransaction();
                try
                {
                    var feedbackBenefitDate = context.FeedbackBenefit.Max(x => x.StartDate);
                    var feedbackBenefitOld = context.FeedbackBenefit.First(x => x.StartDate == feedbackBenefitDate);
                    feedbackBenefitOld.EndDate = DateTime.Now;

                    var feedbackBenefit = new FeedbackBenefit
                    {
                        StartDate = DateTime.Now,
                        Coin = coin,
                    };
                    context.FeedbackBenefit.Add(feedbackBenefit);
                    context.SaveChanges();

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception(ex.Message);
                }

            }
        }
    }
}
