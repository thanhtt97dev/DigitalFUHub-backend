using AutoMapper;
using BusinessObject.Entities;
using DataAccess.IRepositories;
using DataAccess.Repositories;
using DigitalFUHubApi.Comons;
using DigitalFUHubApi.Services;
using DTOs.Seller;
using DTOs.Shop;
using DTOs.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace DigitalFUHubApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class SellersController : ControllerBase
	{
		private readonly IConfiguration _configuration;
		private readonly IProductRepository _productRepository;
		private readonly StorageService _storageService;
		private readonly IShopRepository _shopRepository;
		private readonly IUserRepository _userRepository;
		private readonly IRoleRepository _roleRepository;
		private readonly JwtTokenService _jwtTokenService;
		private readonly IMapper _mapper;

		public SellersController(IConfiguration configuration, IProductRepository productRepository, StorageService storageService, IShopRepository shopRepository, IUserRepository userRepository, IRoleRepository roleRepository, JwtTokenService jwtTokenService, IMapper mapper)
		{
			_configuration = configuration;
			_productRepository = productRepository;
			_storageService = storageService;
			_shopRepository = shopRepository;
			_userRepository = userRepository;
			_roleRepository = roleRepository;
			_jwtTokenService = jwtTokenService;
			_mapper = mapper;
		}


		#region Get All Product with shopId (userId)
		//[Authorize("Seller")]
		[HttpGet("GetAllProduct/{id}")]
		public IActionResult GetAllProduct(int id)
		{
			try
			{
				if (id == 0) return BadRequest();
				var products = _productRepository.GetAllProduct(id);
				return Ok(products);
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Get Product Variants
		[HttpGet("GetProductVariants/{id}")]
		public IActionResult GetProductVariants(int id)
		{
			try
			{
				if (id == 0) return BadRequest();
				var products = _productRepository.GetProductVariants(id);
				return Ok(products);
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Add new product
		/// <summary>
		/// 
		/// </summary>
		/// <param name="request"></param>
		/// <returns>response</returns>
		[HttpPost("Product/New")]
		public async Task<ActionResult<ResponseData>> AddProduct([FromForm] AddProductRequestDTO request)
		{
			ResponseData response = new ResponseData();
			if (!ModelState.IsValid)
			{
				response.Status = new Status
				{
					Ok = false,
					ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT,
					Message = "Dữ liệu không hợp lệ"
				};
				response.Result = new();
				return response;
			}
			string[] fileExtension = new string[] { ".jpge", ".png", ".jpg" };
			if (request.DataVariants.Any(x => !x.FileName.Contains(".xlsx"))
				||
				request.Images.Any(x => !fileExtension.Contains(x.FileName.Substring(x.FileName.LastIndexOf("."))))
				||
				!fileExtension.Contains(request.Thumbnail.FileName.Substring(request.Thumbnail.FileName.LastIndexOf("."))))
			{
				response.Status = new Status
				{
					Ok = false,
					ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT,
					Message = "File không hợp lệ."
				};
				response.Result = new();
				return response;
			}
			try
			{
				DateTime now;
				string filename;
				List<Tag> tags = new List<Tag>();
				request.Tags.ForEach(value =>
				{
					tags.Add(new Tag
					{
						TagName = value
					});
				});
				List<ProductVariant> productVariants = new List<ProductVariant>();
				for (int i = 0; i < request.DataVariants.Count; i++)
				{
					List<AssetInformation> assetInformation = Util.Instance.ReadDataFileExcelProductVariant(request.DataVariants[i]);

					productVariants.Add(new ProductVariant
					{
						Name = request.NameVariants[i],
						Price = request.PriceVariants[i],
						AssetInformation = assetInformation,
						isActivate = true,
					});
				}
				List<ProductMedia> productMedias = new List<ProductMedia>();

				foreach (var file in request.Images)
				{
					now = DateTime.Now;
					filename = string.Format("{0}_{1}{2}{3}{4}{5}{6}{7}", request.UserId, now.Year, now.Month, now.Day, now.Millisecond, now.Second, now.Minute, now.Hour);
					string url = await _storageService.UploadFileToAzureAsync(file, filename);
					productMedias.Add(new ProductMedia
					{
						Url = url
					});
				}

				now = DateTime.Now;
				filename = string.Format("{0}_{1}{2}{3}{4}{5}{6}{7}", request.UserId, now.Year, now.Month, now.Day, now.Millisecond, now.Second, now.Minute, now.Hour);
				string urlThumbnail = await _storageService.UploadFileToAzureAsync(request.Thumbnail, filename);
				Product product = new Product()
				{
					CategoryId = request.Category,
					Description = request.Description,
					Discount = request.Discount,
					ProductName = request.ProductName,
					ShopId = request.UserId,
					Tags = tags,
					Thumbnail = urlThumbnail,
					ProductVariants = productVariants,
					ProductMedias = productMedias,
					ProductStatusId = Constants.PRODUCT_ACTIVE,
					UpdateDate = DateTime.Now,
				};

				await _productRepository.AddProductAsync(product);
				response.Status = new Status
				{
					Ok = true,
					ResponseCode = Constants.RESPONSE_CODE_SUCCESS,
					Message = ""
				};
				response.Result = new();
			}
			catch (Exception e)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
			}
			return response;
		}
		#endregion

		#region register seller
		/// <summary>
		///	
		/// </summary>
		/// <param name="request"></param>
		/// <returns>response</returns>
		[HttpPost("Register")]
		public async Task<ActionResult<ResponseData>> Register(RegisterShopRequestDTO request)
		{
			ResponseData response = new ResponseData();
			if (!ModelState.IsValid)
			{
				response.Status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
				response.Status.Message = "Vui lòng kiểm tra lại dữ liệu.";
				response.Status.Ok = false;
				return response;
			}
			User? user;
			try
			{
				await _shopRepository.CreateShopAsync(request);
				user = _userRepository.GetUserById(request.UserId);
				if (user == null) throw new Exception("Người dùng không khả dụng.");
				user.RoleId = Constants.SELLER_ROLE;
				user.Role = null!;
				await _userRepository.UpdateUser(user);
			}
			catch (Exception e)
			{
				response.Status.ResponseCode = Constants.RESPONSE_CODE_FAILD;
				response.Status.Message = e.Message;
				response.Status.Ok = false;
				return response;
			}


			response.Status.ResponseCode = Constants.RESPONSE_CODE_SUCCESS;
			response.Status.Message = "Đăng ký cửa hàng thành công.";
			response.Status.Ok = true;
			return response;
		}
		#endregion
	}
}
