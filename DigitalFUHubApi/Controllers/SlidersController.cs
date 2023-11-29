using Comons;
using DataAccess.IRepositories;
using DataAccess.Repositories;
using DigitalFUHubApi.Comons;
using DigitalFUHubApi.Services;
using DTOs.Slider;
using DTOs.WishList;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DigitalFUHubApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SlidersController : ControllerBase
    {
        private readonly ISliderRepository sliderRepository;

        public SlidersController(ISliderRepository sliderRepository)
        {
            this.sliderRepository = sliderRepository;
        }

        [HttpPost("GetAll")]
        //[Authorize("Admin")]
        public IActionResult GetAllSliders (SliderAdminRequestParamDTO request)
        {
            try
            {
                if (request.StatusActive != Constants.STATUS_ALL_SLIDER_FOR_FILTER
                    &&
                    request.StatusActive != Constants.STATUS_ACTIVE_SLIDER_FOR_FILTER
                    &&
                    request.StatusActive != Constants.STATUS_UN_ACTIVE_SLIDER_FOR_FILTER)
                {
                    return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid param!", false, new()));
                }

                if (request.Page <= 0)
                {
                    return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid param!", false, new()));
                }

                var numberSliders = sliderRepository.GetNumberSliderByConditions(request.Name, request.Link, request.StartDate, request.EndDate, request.StatusActive);
                var numberPages = numberSliders / Constants.PAGE_SIZE_SLIDER + 1;
                if (request.Page > numberPages)
                {
                    return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid number page", false, new()));
                }

                var sliders = sliderRepository.GetSliders(request.Name, request.Link, request.StartDate, request.EndDate, request.StatusActive, request.Page);

                var result = new 
                {
                    TotalSlider = numberSliders,
                    Sliders = sliders
                };

                return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "SUCCESS", true, result));

            } catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
