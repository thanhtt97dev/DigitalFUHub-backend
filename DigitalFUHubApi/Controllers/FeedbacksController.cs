using AutoMapper;
using BusinessObject.Entities;
using Comons;
using DataAccess.IRepositories;
using DataAccess.Repositories;
using DigitalFUHubApi.Comons;
using DigitalFUHubApi.Services;
using DTOs.Feedback;
using DTOs.Seller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace DigitalFUHubApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class FeedbacksController : ControllerBase
	{
		private readonly IOrderRepository _orderRepository;
		private readonly IFeedbackRepository _feedbackRepository;
		private readonly StorageService _storageService;
		private readonly JwtTokenService _jwtTokenService;
		private readonly IMapper _mapper;

		public FeedbacksController(IOrderRepository orderRepository, IFeedbackRepository feedbackRepository, StorageService storageService, JwtTokenService jwtTokenService, IMapper mapper)
		{
			_orderRepository = orderRepository;
			_feedbackRepository = feedbackRepository;
			_storageService = storageService;
			_jwtTokenService = jwtTokenService;
			_mapper = mapper;
		}


		#region Get feedbacks for customer
		[HttpPost("search")]
		public IActionResult Search(SearchFeedbackRequestDTO request)
		{
			if (!ModelState.IsValid) return BadRequest();
			if (!Constants.FEEDBACK_TYPES.Contains(request.Type))
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid feedback type", false, new()));
			}
			try
			{
				int numberFeedBacks = _feedbackRepository.GetNumberFeedbackWithCondition(request.ProductId, request.Type, request.Page);
				var numberPages = numberFeedBacks / Constants.PAGE_SIZE_FEEDBACK + 1;

				if (request.Page > numberPages)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid number page", false, new()));
				}

				List<Feedback> feedbacks = _feedbackRepository.GetFeedbacksWithCondition(request.ProductId, request.Type, request.Page);

				var result = new SearchFeedbackResponseDTO
				{
					TotalFeedback = numberFeedBacks,
					Feedbacks = _mapper.Map<List<SearchFeedbackDetailResponseDTO>>(feedbacks)
				};

				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Succes", true, result));
			}
			catch (ArgumentException ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

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
		[HttpPost("Customer/Add")]
		public async Task<IActionResult> AddFeedbackOrder([FromForm] CustomerFeedbackOrderRequestDTO request)
		{

			try
			{
				if (request.UserId != _jwtTokenService.GetUserIdByAccessToken(User))
				{
					return Unauthorized();
				}
				if (!ModelState.IsValid)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid data", false, new()));
				}

				var orderDetail = _orderRepository.GetOrderDetail(request.OrderDetailId);
				if (orderDetail == null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Not found", false, new()));
				}

				if (orderDetail.Order.OrderStatusId != Constants.ORDER_STATUS_CONFIRMED)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_FEEDBACK_ORDER_UN_COMFIRM, "Order's status confirm not yet", false, new()));
				}

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


				int bonusCoin = _feedbackRepository.AddFeedbackOrder(request.UserId, request.OrderId, request.OrderDetailId, request.Content, request.Rate, urlImages);
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, bonusCoin));
			}
			catch(ArgumentOutOfRangeException)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_FEEDBACK_AGAIN, "Not feedback again", false, new()));
			}
			catch (Exception e)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
			}
		}
		#endregion

		#region get feedback detail
		[HttpGet("Customer/{userId}/{orderId}")]
		public IActionResult GetFeedbackDetailOrder(long userId, long orderId)
		{
			try
			{
				Order? order = _feedbackRepository.GetFeedbackDetail(orderId, userId);
				if (order == null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Not found", false, new()));
				}
				List<CustomerFeedbackDetailOrderResponseDTO> response = order.OrderDetails.Where(x => x.IsFeedback == true).Select(x => new CustomerFeedbackDetailOrderResponseDTO
				{
					Username = order.User.Username,
					Avatar = order.User.Avatar,
					ProductName = x.ProductVariant.Product.ProductName ?? "",
					ProductVariantName = x.ProductVariant.Name ?? "",
					Content = x?.Feedback?.Content ?? "",
					Rate = x.Feedback?.Rate ?? 0,
					Quantity = x.Quantity,
					Date = x.Feedback?.DateUpdate ?? new DateTime(),
					Thumbnail = x.ProductVariant.Product.Thumbnail ?? "",
					UrlImages = x.Feedback == null || x.Feedback?.FeedbackMedias == null ? new List<string>() : x.Feedback.FeedbackMedias.Select(x => x.Url).ToList(),
				}).ToList();
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, response));
			}
			catch (Exception e)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
			}

		}
		#endregion

		#region get list feedback of seller
		[Authorize("Seller")]
		[HttpPost("Seller/List")]
		public IActionResult GetListFeedbackSeller(SellerFeedbackRequestDTO request)
		{
			try
			{
				int[] rates = new[] { 0, 1, 2, 3, 4, 5 };
				if (!ModelState.IsValid || !rates.Contains(request.Rate))
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid data", false, new()));
				}
				if (request.Page <= 0)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid page", false, new()));
				}

				DateTime? fromDate = string.IsNullOrWhiteSpace(request.FromDate) ? null :
					DateTime.ParseExact(request.FromDate, "M/d/yyyy", CultureInfo.InvariantCulture);
				DateTime? toDate = string.IsNullOrWhiteSpace(request.ToDate) ? null :
					DateTime.ParseExact(request.ToDate, "M/d/yyyy", CultureInfo.InvariantCulture);
				if (fromDate >= toDate)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid date", false, new()));
				}
				(long totalItems, List<Order> orders) = _feedbackRepository.GetListFeedbackSeller(request.UserId,
					request.OrderId, request.UserName.Trim(), request.ProductName.Trim(), request.ProductVariantName.Trim(),
					fromDate, toDate, request.Rate, request.Page);
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, new ListFeedbackResponseDTO
				{
					TotalItems = totalItems,
					Feedbacks = _mapper.Map<List<SellerFeedbackResponseDTO>>(orders)
				}));
			}
			catch (Exception e)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
			}
		}
		#endregion
	}
}
