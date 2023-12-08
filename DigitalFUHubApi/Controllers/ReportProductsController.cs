using AutoMapper;
using BusinessObject.Entities;
using Comons;
using DataAccess.IRepositories;
using DataAccess.Repositories;
using DigitalFUHubApi.Comons;
using DigitalFUHubApi.Services;
using DTOs.Admin;
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

		#region Get Report Products
		[Authorize("Admin")]
		[HttpPost("admin/all")]
		public IActionResult GetReportProducts(GetReportProductRequestDTO request)
		{
			if (!ModelState.IsValid) return BadRequest();

			if (!Constants.REPORT_PRODUCT_STATUS.Contains(request.ReportProductStatusId))
				return BadRequest();

			try
			{
				(bool isValid, DateTime? fromDate, DateTime? toDate) = Util.GetFromDateToDate(request.FromDate, request.ToDate);
				if (!isValid)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid date", false, new()));
				}

				long productId;
				long.TryParse(request.ProductId, out productId);

				var totalRecord = reportProductRepository.GetNumberReportProductByCondition
					(request.Email, productId, request.ProductName, request.ShopName, fromDate, toDate, 
					request.ReasonReportProductId, request.ReportProductStatusId);

				var numberPages = totalRecord / Constants.PAGE_SIZE + 1;
				if (request.Page > numberPages || request.Page == 0)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid number page", false, new()));
				}

				var reportProducts = reportProductRepository.GetReportProducts
					(request.Email, productId, request.ProductName, request.ShopName, fromDate, toDate,
					request.ReasonReportProductId, request.ReportProductStatusId, request.Page);

				var result = new
				{
					Total = totalRecord,
					ReportProducts = mapper.Map<List<GetReportProductResponseDTO>>(reportProducts)
				};
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, result));
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Update Report Product Status
		[Authorize("Admin")]
		[HttpPost("admin/update")]
		public IActionResult UpdateReportProductStatus(UpdateReportProductRequestDTO request)
		{
			if (!ModelState.IsValid) return BadRequest();

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
		#endregion

		#region Add report product
		[HttpPost("add")]
		[Authorize]
		public IActionResult AddReportProduct([FromForm] AddReportProductRequestDTO request)
		{
			try
			{
				if (!ModelState.IsValid) return BadRequest();

				int minLength = 10;
				int maxLength = 50;

				if (request.UserId != jwtTokenService.GetUserIdByAccessToken(User)) return Unauthorized();

				var user = userRepository.GetUserById(request.UserId);
				if (user == null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "user not found!!", false, new()));
				}

				var product = productRepository.GetProductEntityById(request.ProductId);
				if (product == null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "product not found!!", false, new()));
				}

				var reason = reasonReportProductRepository.GetEntityById(request.ReasonReportProductId);
				if (reason == null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "reason not found!!", false, new()));
				}

				string trimmedDescription = Regex.Replace(request.Description, @"\s+", " ");

				if (trimmedDescription.Length < minLength || trimmedDescription.Length > maxLength)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Description for this reason should have 10 - 50 characters!", false, new()));
				}

				// Ok
				ReportProduct reportProduct = mapper.Map<ReportProduct>(request);
				reportProduct.DateCreate = DateTime.Now;
				reportProduct.ReportProductStatusId = Constants.REPORT_PRODUCT_STATUS_VERIFYING;
				reportProductRepository.AddReportProduct(reportProduct);

				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success!", true, new()));

			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

	}
}
