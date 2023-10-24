using AutoMapper;
using BusinessObject.Entities;
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
		#region Get all feedback
		[HttpGet("GetAll")]
		public IActionResult GetAll(long productId)
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
		#endregion

		#region add feedback order
		[Authorize("Customer,Seller")]
		[HttpPost("Add")]
		public async Task<IActionResult> AddFeedbackOrder([FromForm] CustomerFeedbackOrderRequestDTO request)
		{
			if (!ModelState.IsValid)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "INVALID", false, new()));
			}
			try
			{
				string[] fileExtension = new string[] { ".jpge", ".png", ".jpg" };
				List<string> urlImages = new List<string>();
				if (request.ImageFiles != null
					&&
					request?.ImageFiles?.Count > 0
					&&
					!request.ImageFiles.Any(x => !fileExtension.Contains(x.FileName.Substring(x.FileName.LastIndexOf(".")))))
				{
					string filename;
					DateTime now;
					foreach (IFormFile file in request.ImageFiles)
					{
						now = DateTime.Now;
						filename = string.Format("{0}_{1}{2}{3}{4}{5}{6}{7}{8}", request.UserId, now.Year, now.Month, now.Day, now.Millisecond, now.Second, now.Minute, now.Hour, file.FileName.Substring(file.FileName.LastIndexOf(".")));
						string path = await _storageService.UploadFileToAzureAsync(file, filename);
						urlImages.Add(path);
					}
				}

				_feedbackRepository.AddFeedbackOrder(request.UserId, request.OrderId, request.OrderDetailId, request.Content, request.Rate, urlImages);
			}
			catch (Exception e)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_FAILD, e.Message, false, new()));
			}
			return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "SUCCESS", true, new()));
		}
		#endregion

		#region get feedback detail
		[Authorize("Customer,Seller")]
		[HttpPost("Detail")]
		public IActionResult FeedbackDetailOrder(CustomerFeedbackDetailOrderRequestDTO request)
		{
			Order? order = _feedbackRepository.GetFeedbackDetail(request.OrderId, request.UserId);
			if (order == null)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "INVALID", false, new()));
			}
			List<CustomerFeedbackDetailOrderResponseDTO> response = order.OrderDetails.Where(x => x.IsFeedback == true).Select(x => new CustomerFeedbackDetailOrderResponseDTO
			{
				Fullname = order.User.Fullname,
				Avatar = order.User.Avatar,
				ProductName = x.ProductVariant.Product.ProductName ?? "",
				ProductVariantName = x.ProductVariant.Name ?? "",
				Content = x?.Feedback?.Content ?? "",
				Rate = x.Feedback?.Rate ?? 0,
				Quantity = x.Quantity,
				Date = x.Feedback?.UpdateDate ?? new DateTime(),
				Thumbnail = x.ProductVariant.Product.Thumbnail ?? "",
				UrlImages = x.Feedback == null || x.Feedback?.FeedbackMedias == null ? new List<string>() : x.Feedback.FeedbackMedias.Select(x => x.Url).ToList(),
			}).ToList();
			return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "SUCCESS", true, response));
		}
		#endregion
	}
}
