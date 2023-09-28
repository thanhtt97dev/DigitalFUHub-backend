using AutoMapper;
using DataAccess.IRepositories;
using DataAccess.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DigitalFUHubApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbacksController : ControllerBase
    {

        private readonly IConfiguration _configuration;
        private readonly IFeedbackRepository _feedbackRepository;
        private readonly IMapper _mapper;

        public FeedbacksController(IConfiguration configuration, IFeedbackRepository feedbackRepository, IMapper mapper)
        {
            _configuration = configuration;
            _feedbackRepository = feedbackRepository;
            _mapper = mapper;
        }

        [HttpGet("GetAll")]
        public IActionResult GetAll (long productId)
        {
            try
            {
                return Ok(_feedbackRepository.GetFeedbacks(productId));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }


    }
}
