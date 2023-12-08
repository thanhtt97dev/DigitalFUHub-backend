using Comons;
using DataAccess.IRepositories;
using DataAccess.Repositories;
using DigitalFUHubApi.Comons;
using DTOs.Admin;
using DTOs.BusinessFee;
using DTOs.FeedbackBenefit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DigitalFUHubApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackBenefitsController : ControllerBase
    {
        private readonly IFeedbackBenefitRepository feedbackBenefitRepository;

        public FeedbackBenefitsController(IFeedbackBenefitRepository feedbackBenefitRepository)
        {
            this.feedbackBenefitRepository = feedbackBenefitRepository;
        }

        #region get feedback benefit
        [Authorize("Admin")]
        [HttpPost("GetFeedbackBenefits")]
        public IActionResult GetFeedbackBenefits(FeedbackBenefitAdminRequestDTO request)
        {
            if (!ModelState.IsValid) return BadRequest();
            try
            {
                (bool isValid, DateTime? fromDate, DateTime? toDate) = Util.GetFromDateToDate(request.FromDate, request.ToDate);
                if (!isValid)
                {
                    return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid date", false, new()));
                }

                long feedbackBenefitId;
                long.TryParse(request.FeedbackBenefitId, out feedbackBenefitId);

                var fees = feedbackBenefitRepository.GetFeedbackBenefits(feedbackBenefitId, request.MaxCoin, fromDate, toDate);
                return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, fees));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        #endregion

        #region Add new feedback benefit
        [Authorize("Admin")]
        [HttpPost("AddNewFeedbackBenefit")]
        public IActionResult AddNewFeedbackBenefit(FeedbackBenefitAdminCreateRequestDTO request)
        {
            if (!ModelState.IsValid) return BadRequest();

            ResponseData responseData = new ResponseData();
            try
            {
                feedbackBenefitRepository.AddNewFeedbackBenefit(request.Coin);

                return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, new { }));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        #endregion
    }
}
