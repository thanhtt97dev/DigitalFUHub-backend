using AutoMapper;
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
        private readonly AzureFilesService azureFilesService;
        private readonly IMapper mapper;

        public SlidersController(ISliderRepository sliderRepository, IProductRepository productRepository, AzureFilesService azureFilesService, IMapper mapper)
        {
            this.sliderRepository = sliderRepository;
            this.productRepository = productRepository;
            this.azureFilesService = azureFilesService;
            this.mapper = mapper;
        }

        #region Get Sliders (Home Page)
        [HttpGet("getAll")]
        public IActionResult GetAllSliders()
        {
            try
            {
                var sliders = sliderRepository.GetSliders();
                
                return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "SUCCESS", true, mapper.Map<List<HomeCustomerSliderResponseDTO>>(sliders)));

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        #endregion

        #region Get Sliders (Admin)
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

                var numberSliders = sliderRepository.GetNumberSliderByConditions(request.StatusActive);
                var numberPages = numberSliders / Constants.PAGE_SIZE_SLIDER + 1;
                if (request.Page > numberPages)
                {
                    return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid number page", false, new()));
                }

                var sliders = sliderRepository.GetSliders(request.StatusActive, request.Page);

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
        #endregion

        #region Add Slider (Admin)
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

                if (string.IsNullOrEmpty(request.Link))
                {
                    return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid param!", false, new()));
                }

                if (!Util.IsUrlValid(request.Link))
                {
                    return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Url invalid!", false, new()));
                }

                if (request.Image == null)
                {
                    return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid param!", false, new()));
                }

                if (request.Image.Length > Constants.UPLOAD_FILE_SIZE_LIMIT)                
                {
                    return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Size file upload exceed 2MB", false, new()));
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
                urlImage = await azureFilesService.UploadFileToAzureAsync(fileRequest, filename);

                // Initial New Slider
                Slider slider = new Slider
                {
                    Name = request.Name,
                    Url = urlImage,
                    Link = request.Link,
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
        #endregion

        #region Get Slider by id (Admin)
        [Authorize("Admin")]
        [HttpGet("admin/getById/{id}")]
        public IActionResult AddSliderById(long id)
        {
            try
            {
               var slider = sliderRepository.GetSliderById(id);

                if (slider == null)
                {
                    return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "slider not found", false, new()));
                }

                return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "SUCCESS", true, mapper.Map<SliderAdminGetByIdResponseDTO>(slider)));


            } catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        #endregion

        #region Update Slider (Admin)
        [Authorize("Admin")]
        [HttpPut("admin/updateSlider")]
        public async Task<IActionResult> UpdateSlider([FromForm] SliderAdminUpdateRequestDTO request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Name))
                {
                    return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid param!", false, new()));
                }

                if (string.IsNullOrEmpty(request.Link))
                {
                    return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid param!", false, new()));
                }

                if (!Util.IsUrlValid(request.Link))
                {
                    return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Url invalid!", false, new()));
                }

                var slider = sliderRepository.GetSliderById(request.SliderId);

                if (slider == null)
                {
                    return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Slider Not found", false, new()));
                }

                if (request.Image != null)
                {
                    if (request.Image.Length > Constants.UPLOAD_FILE_SIZE_LIMIT)
                    {
                        return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Size file upload exceed 2MB", false, new()));
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
                    urlImage = await azureFilesService.UploadFileToAzureAsync(fileRequest, filename);

                    string urlOld = slider.Url;

                    // update new url
                    slider.Url = urlImage;

                    // delete url old
                    await azureFilesService.RemoveFileFromAzureAsync(urlOld.Substring(urlOld.LastIndexOf("/") + 1));
                }

                // Update slider
                slider.Name = request.Name;
                slider.Link = request.Link;
                slider.IsActive = request.IsActive;

                // Add Slider
                sliderRepository.UpdateSlider(slider);

                return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "SUCCESS", true, new()));

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        #endregion

        #region Delete Slider (Admin)
        [Authorize("Admin")]
        [HttpPost("admin/delete")]
        public IActionResult DeleteSlider(long sliderId)
        {
            try
            {
                var slider = sliderRepository.GetSliderById(sliderId);

                if (slider == null)
                {
                    return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "slider not found", false, new()));
                }

                sliderRepository.DeleteSlider(slider);
                return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "SUCCESS", true, new()));

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        #endregion
    }
}
