using BusinessObject.Entities;
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
        private readonly IProductRepository productRepository;
        private readonly StorageService storageService;

        public SlidersController(ISliderRepository sliderRepository, IProductRepository productRepository, StorageService storageService)
        {
            this.sliderRepository = sliderRepository;
            this.productRepository = productRepository;
            this.storageService = storageService;
        }


        [Authorize("Admin")]
        [HttpPost("admin/getSliders")]
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


        [Authorize("Admin")]
        [HttpPost("admin/addSlider")]
        public async Task<IActionResult> AddSlider([FromForm] SliderAdminAddRequestDTO request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Name))
                {
                    return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid param!", false, new()));
                }

                if (request.Image == null)
                {
                    return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid param!", false, new()));
                }

                if (request.Image.Length > Constants.UPLOAD_FILE_SIZE_LIMIT)                
                {
                    return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Size file upload exceed 2MB", false, new()));
                }

                var product = productRepository.GetProductEntityById(request.ProductId);

                if (product == null)
                {
                    return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Product not found", false, new()));
                }

                // Declares variable
                string[] fileExtension = new string[] { ".jpge", ".png", ".jpg" };
                string urlImage = "";

                // Check file extension
                IFormFile fileRequest = request.Image;
                if (!fileExtension.Contains(fileRequest.FileName.Substring(fileRequest.FileName.LastIndexOf("."))))
                {
                    return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid file!", false, new()));
                }

                // Declares variable
                DateTime now;
                string filename = "";

                // Upload file to azure
                now = DateTime.Now;
                filename = string.Format("{0}_{1}{2}{3}{4}{5}{6}{7}{8}", Constants.ADMIN_USER_ID, now.Year, now.Month, now.Day, now.Millisecond, now.Second, now.Minute, now.Hour, fileRequest.FileName.Substring(fileRequest.FileName.LastIndexOf(".")));
                urlImage = await storageService.UploadFileToAzureAsync(fileRequest, filename);

                // Initial New Slider
                Slider slider = new Slider
                {
                    Name = request.Name,
                    Url = urlImage,
                    Link = "/product/" + request.ProductId,
                    DateCreate = DateTime.Now,
                    IsActive = request.IsActive
                };

                // Add Slider
                sliderRepository.AddSlider(slider);

                return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "SUCCESS", true, new()));

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
