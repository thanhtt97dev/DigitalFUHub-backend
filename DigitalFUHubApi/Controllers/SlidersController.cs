using DataAccess.IRepositories;
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
    }
}
