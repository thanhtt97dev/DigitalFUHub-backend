using AutoMapper;
using BusinessObject.Entities;
using DataAccess.IRepositories;
using DataAccess.Repositories;
using DigitalFUHubApi.Comons;
using DigitalFUHubApi.Services;
using DTOs.Product;
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

		#region Get Product Of Seller
		/// <summary>
		/// 
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="productId"></param>
		/// <returns></returns>
		[HttpGet("{userId}/GetProduct/{productId}")]
		public ActionResult<ResponseData> GetProductVariants(long userId, long productId)
		{
			ResponseData response = new ResponseData();
			// check user have shop
			bool existShop = _shopRepository.UserHasShop(userId);
			if (!existShop)
			{
				response.Status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
				response.Status.Ok = false;
				response.Status.Message = "Cửa hàng không tồn tại.";
			}
			else
			{
				// check shop have product
				bool existProduct = _shopRepository.ShopHasProduct(userId, productId);
				if (!existProduct)
				{
					response.Status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
					response.Status.Ok = false;
					response.Status.Message = "Cửa hàng không tồn tại sản phẩm này.";
				}
				else
				{
					// get product
					Product product = _shopRepository.GetProductById(productId);
					if (product.ProductStatusId == Constants.PRODUCT_BAN)
					{
						response.Status.ResponseCode = Constants.RESPONSE_CODE_FAILD;
						response.Status.Ok = false;
						response.Status.Message = "Sản phẩm đã vi phạm chính sách của sàn.";
					}
					else if (product.ProductStatusId == Constants.PRODUCT_HIDE)
					{
						response.Status.ResponseCode = Constants.RESPONSE_CODE_FAILD;
						response.Status.Ok = false;
						response.Status.Message = "Sản phẩm đã bị xóa.";
					}
					else
					{
						response.Status.ResponseCode = Constants.RESPONSE_CODE_SUCCESS;
						response.Status.Ok = true;
						response.Status.Message = "";
						response.Result = _mapper.Map<ProductResponseDTO>(product);
					}
				}
			}
			return response;
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

				foreach (IFormFile file in request.Images)
				{
					now = DateTime.Now;
					filename = string.Format("{0}_{1}{2}{3}{4}{5}{6}{7}{8}", request.UserId, now.Year, now.Month, now.Day, now.Millisecond, now.Second, now.Minute, now.Hour, file.FileName.Substring(file.FileName.LastIndexOf(".")));
					string url = await _storageService.UploadFileToAzureAsync(file, filename);
					productMedias.Add(new ProductMedia
					{
						Url = url
					});
				}

				now = DateTime.Now;
				filename = string.Format("{0}_{1}{2}{3}{4}{5}{6}{7}{8}", request.UserId, now.Year, now.Month, now.Day, now.Millisecond, now.Second, now.Minute, now.Hour, request.Thumbnail.FileName.Substring(request.Thumbnail.FileName.LastIndexOf(".")));
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

				_productRepository.AddProduct(product);
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

		#region Edit product
		#endregion
		[HttpPut("Product/Edit/{productId}")]
		public async Task<ActionResult<ResponseData>> EditProduct(long productId, [FromForm] EditProductRequestDTO request)
		{
			ResponseData response = new ResponseData();
			//if (!ModelState.IsValid)
			//{
			//	response.Status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
			//	response.Status.Ok = false;
			//	response.Status.Message = "Vui lòng kiểm tra lại dữ liệu.";
			//	return response;
			//}
			if (productId != request.ProductId)
			{
				response.Status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
				response.Status.Ok = false;
				response.Status.Message = "Sản phẩm không hợp lệ.";
				return response;
			}
			try
			{
				string filename = "";
				DateTime now;
				List<Tag> tags = new List<Tag>();
				request.Tags.ForEach((value) =>
				{
					tags.Add(new Tag { ProductId = request.ProductId, TagName = value });
				});

				List<ProductMedia> productMediaNew = new List<ProductMedia>();
				foreach (var file in request.ProductImagesNew)
				{
					now = DateTime.Now;
					filename = string.Format("{0}_{1}{2}{3}{4}{5}{6}{7}{8}", request.UserId, now.Year, now.Month, now.Day, now.Millisecond, now.Second, now.Minute, now.Hour, file.FileName.Substring(file.FileName.LastIndexOf(".")));
					string url = await _storageService.UploadFileToAzureAsync(file, filename);
					productMediaNew.Add(new ProductMedia
					{
						ProductId = request.ProductId,
						Url = url,
					});
				};

				List<ProductVariant> productVariantsUpdate = new List<ProductVariant>();
				for (int i = 0; i < request.ProductVariantIdUpdate.Count; i++)
				{
					productVariantsUpdate.Add(new ProductVariant
					{
						Name = request.ProductVariantNameUpdate[i],
						Price = request.ProductVariantPriceUpdate[i],
						ProductId = request.ProductId,
						ProductVariantId = request.ProductVariantIdUpdate[i],
						AssetInformation = request.ProductVariantFileUpdate.Count == 0 || request.ProductVariantFileUpdate[i] == null ? null : Util.Instance.ReadDataFileExcelProductVariant(request.ProductVariantFileUpdate[i]),
					});
				}
				List<ProductVariant> productVariantsNew = new List<ProductVariant>();
				for (int i = 0; i < request.ProductVariantFileNew.Count; i++)
				{
					productVariantsNew.Add(new ProductVariant
					{
						AssetInformation = Util.Instance.ReadDataFileExcelProductVariant(request.ProductVariantFileNew[i]),
						isActivate = true,
						Name = request.ProductVariantNameNew[i],
						Price = request.ProductVariantPriceNew[i],
						ProductId = request.ProductId,
					});
				}
				string urlThumbnailOld = _productRepository.GetProductThumbnail(request.ProductId);
				string urlThumbnailNew = "";
				if (request.ProductThumbnail != null)
				{
					now = DateTime.Now;
					filename = string.Format("{0}_{1}{2}{3}{4}{5}{6}{7}{8}", request.UserId, now.Year, now.Month, now.Day, now.Millisecond, now.Second, now.Minute, now.Hour, request.ProductThumbnail.FileName.Substring(request.ProductThumbnail.FileName.LastIndexOf(".")));
					urlThumbnailNew = await _storageService.UploadFileToAzureAsync(request.ProductThumbnail, filename);
				}
				
				Product product = new Product
				{
					ProductId = request.ProductId,
					ProductName = request.ProductName,
					Description = request.ProductDescription,
					Discount = request.Discount,
					CategoryId = request.CategoryId,
					Thumbnail = request.ProductThumbnail == null ? null : urlThumbnailNew
				};
				List<ProductMedia> productMedia = _productRepository.GetAllProductMediaById(request.ProductId);

				_productRepository.EditProduct(product, productVariantsNew, productVariantsUpdate, tags, productMediaNew, request.ProductImagesOld);

				// delete thumbnail old
				if (request.ProductThumbnail != null)
				{
					await _storageService.RemoveFileFromAzureAsync(urlThumbnailOld.Substring(urlThumbnailOld.LastIndexOf("/") + 1));
				}
				// delete image product
				if (productMedia.Count(x => !request.ProductImagesOld.Any(m => m == x.Url)) > 0)
				{
					List<ProductMedia> productMediaDelete = productMedia.Where(x => !request.ProductImagesOld.Any(m => m == x.Url)).ToList();
					foreach (ProductMedia media in productMediaDelete)
					{
						await _storageService.RemoveFileFromAzureAsync(media.Url.Substring(media.Url.LastIndexOf("/") + 1));
					}
				}
			}
			catch (Exception)
			{
				response.Status.ResponseCode = Constants.RESPONSE_CODE_FAILD;
				response.Status.Ok = false;
				response.Status.Message = "Đã có lỗi xảy ra.";
				return response;
			}
			response.Status.ResponseCode = Constants.RESPONSE_CODE_SUCCESS;
			response.Status.Ok = true;
			response.Status.Message = "";
			return response;
		}

		#region register seller
		/// <summary>
		///	
		/// </summary>
		/// <param name="request"></param>
		/// <returns>response</returns>
		[HttpPost("Register")]
		public  ActionResult<ResponseData> Register(RegisterShopRequestDTO request)
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
				 _shopRepository.CreateShop(request);
				user = _userRepository.GetUserById(request.UserId);
				if (user == null) throw new Exception("Người dùng không khả dụng.");
				user.RoleId = Constants.SELLER_ROLE;
				user.Role = null!;
				 _userRepository.UpdateUser(user);
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
