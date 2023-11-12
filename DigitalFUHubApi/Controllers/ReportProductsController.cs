using AutoMapper;
using BusinessObject.Entities;
using Comons;
using DataAccess.IRepositories;
using DataAccess.Repositories;
using DigitalFUHubApi.Comons;
using DigitalFUHubApi.Services;
using DTOs.Product;
using DTOs.ReportProduct;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace DigitalFUHubApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ReportProductsController : ControllerBase
	{
		private readonly IReportProductRepository reportProductRepository;
		private readonly IMapper mapper;
        private readonly JwtTokenService jwtTokenService;
        private readonly IUserRepository userRepository;
        private readonly IProductRepository productRepository;
        private readonly IReasonReportProductRepository reasonReportProductRepository;

        public ReportProductsController(
            IReportProductRepository reportProductRepository, 
            IMapper mapper, 
            JwtTokenService jwtTokenService,
            IUserRepository userRepository,
            IProductRepository productRepository,
            IReasonReportProductRepository reasonReportProductRepository)
		{
			this.reportProductRepository = reportProductRepository;
			this.mapper = mapper;
            this.jwtTokenService = jwtTokenService;
            this.userRepository = userRepository;
            this.productRepository = productRepository;
            this.reasonReportProductRepository = reasonReportProductRepository;

        }

		[Authorize("Admin")]
		[HttpPost("admin/update")]
		public IActionResult GetProductDetailAdmin(UpdateReportProductRequestDTO request)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest();
			}
			try
			{
				reportProductRepository.UpdateReportProduct(request.ReportProductId, request.Status, request.Note);
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "SUCCESS", true, new()));
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}

        [HttpPost("add")]
        [Authorize]
        public IActionResult AddReportProduct ([FromForm] AddReportProductRequestDTO request)
        {
            ResponseData responseData = new ResponseData();
            Status status = new Status();
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }

                int minLength = 10;
                int maxLength = 50;

                if (request.UserId != jwtTokenService.GetUserIdByAccessToken(User))
                {
                    return Unauthorized();
                }

                var user = userRepository.GetUserById(request.UserId);

                if (user == null)
                {
                    status.ResponseCode = Constants.RESPONSE_CODE_DATA_NOT_FOUND;
                    status.Ok = false;
                    status.Message = "user not found!!";
                    responseData.Status = status;
                    return Ok(responseData);
                }

                var product = productRepository.GetProductEntityById(request.ProductId);

                if (product == null)
                {
                    status.ResponseCode = Constants.RESPONSE_CODE_DATA_NOT_FOUND;
                    status.Ok = false;
                    status.Message = "product not found!!";
                    responseData.Status = status;
                }

                var reason = reasonReportProductRepository.GetEntityById(request.ReasonReportProductId);

                if (reason == null)
                {
                    status.ResponseCode = Constants.RESPONSE_CODE_DATA_NOT_FOUND;
                    status.Ok = false;
                    status.Message = "reason not found!!";
                    responseData.Status = status;
                }

                string trimmedDescription = Regex.Replace(request.Description, @"\s+", " ");

                if (trimmedDescription.Length < minLength || trimmedDescription.Length > maxLength)
                {
                    status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
                    status.Ok = false;
                    status.Message = "Description for this reason should have 10 - 50 characters";
                    responseData.Status = status;
                }

                // Ok
                ReportProduct reportProduct = mapper.Map<ReportProduct>(request);
                reportProduct.DateCreate = DateTime.Now;
                reportProduct.ReportProductStatusId = Constants.REPORT_PRODUCT_STATUS_VERIFYING;
                reportProductRepository.AddReportProduct(reportProduct);

                status.ResponseCode = Constants.RESPONSE_CODE_SUCCESS;
                status.Message = "Success";
                status.Ok = true;
                responseData.Status = status;
                return Ok(responseData);

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
