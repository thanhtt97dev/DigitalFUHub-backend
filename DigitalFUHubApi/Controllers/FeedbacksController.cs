using AutoMapper;
using Comons;
using DataAccess.IRepositories;
using DataAccess.Repositories;
using DigitalFUHubApi.Comons;
using DigitalFUHubApi.Services;
using DTOs.Feedback;
using Microsoft.AspNetCore.Authorization;
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
        private readonly StorageService _storageService;
        private readonly IMapper _mapper;

		public FeedbacksController(IConfiguration configuration, IFeedbackRepository feedbackRepository, StorageService storageService, IMapper mapper)
		{
			_configuration = configuration;
			_feedbackRepository = feedbackRepository;
			_storageService = storageService;
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

        [Authorize("Customer,Seller")]
        [HttpPost("Order")]
        public async Task<IActionResult> FeedbackOrder([FromForm] CustomerFeedbackOrderRequestDTO request)
        {
            if(!ModelState.IsValid)
            {
                return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "INVALID", false, new()));
            }
            try
            {
				string[] fileExtension = new string[] { ".jpge", ".png", ".jpg" };
				List<string> urlImages = new List<string>();
                if(request.ImageFiles != null || request?.ImageFiles?.Count > 0)
                {
                    if(request.ImageFiles.Any(x => !fileExtension.Contains(x.FileName.Substring(x.FileName.LastIndexOf(".")))))
                    {
                        throw new Exception("File extension not accept.");
                    } else
                    {
                        string filename;
                        DateTime now;
                        foreach (IFormFile file in request.ImageFiles)
                        {
							now = DateTime.Now;
							filename = string.Format("{0}_{1}{2}{3}{4}{5}{6}{7}{8}", request.UserId, now.Year, now.Month, now.Day, now.Millisecond, now.Second, now.Minute, now.Hour, file.FileName.Substring(file.FileName.LastIndexOf(".")));
                            await _storageService.UploadFileToAzureAsync(file, filename);
							urlImages.Add(filename);
						}
					}

				}

                _feedbackRepository.FeedbackOrder(request.UserId, request.OrderId, request.OrderDetailId, request.Content, request.Rate, urlImages);
            }
            catch (Exception)
            {
				return Ok(new ResponseData(Constants.RESPONSE_CODE_FAILD, "ERROR", false, new()));
			}
            return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "SUCCESS", true, new()));
        }
    }
}
