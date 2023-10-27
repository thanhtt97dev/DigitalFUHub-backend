using AutoMapper;
using BusinessObject.Entities;
using Comons;
using DataAccess.IRepositories;
using DataAccess.Repositories;
using DigitalFUHubApi.Comons;
using DigitalFUHubApi.Services;
using DTOs.Product;
using DTOs.Seller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DigitalFUHubApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ProductsController : ControllerBase
	{
		private readonly IConfiguration _configuration;
		private readonly IProductRepository _productRepository;
		private readonly StorageService _storageService;
		private readonly IShopRepository _shopRepository;
		private readonly IUserRepository _userRepository;
		private readonly IRoleRepository _roleRepository;
		private readonly IOrderRepository _orderRepository;
		private readonly ICouponRepository _couponRepository;
		private readonly JwtTokenService _jwtTokenService;
		private readonly IMapper _mapper;

		public ProductsController(IConfiguration configuration, IProductRepository productRepository, StorageService storageService, IShopRepository shopRepository, IUserRepository userRepository, IRoleRepository roleRepository, IOrderRepository orderRepository, ICouponRepository couponRepository, JwtTokenService jwtTokenService, IMapper mapper)
		{
			_configuration = configuration;
			_productRepository = productRepository;
			_storageService = storageService;
			_shopRepository = shopRepository;
			_userRepository = userRepository;
			_roleRepository = roleRepository;
			_orderRepository = orderRepository;
			_couponRepository = couponRepository;
			_jwtTokenService = jwtTokenService;
			_mapper = mapper;
		}

		[HttpGet("GetById/{productId}")]
		public IActionResult GetById(long productId)
		{
			try
			{
				if (productId == 0)
				{
					return BadRequest(new Status());
				}

				var product = _productRepository.GetProductById(productId);
				if (product == null)
				{
					return NotFound(new Status());
				}
				return Ok(product);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return BadRequest(new Status());
			}
		}
		#region Get All Product with shopId (userId)
		[Authorize("Seller")]
		[HttpGet("Seller/{userId}/All")]
		public IActionResult GetAllProductSeller(long userId)
		{
			try
			{
				if (userId != _jwtTokenService.GetUserIdByAccessToken(User))
				{
					return Unauthorized();
				}
				var products = _productRepository.GetAllProduct(userId);
				return Ok(products);
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Get All Product 
		[HttpGet("GetAllProduct")]
		public IActionResult GetAllProduct()
		{
			try
			{
				var products = _productRepository.GetAllProduct();
				return Ok(products);
			}
			catch (Exception ex)
			{
				return BadRequest(new { Message = ex.Message });
			}
		}
		#endregion

		#region Get Product Of Seller
		[Authorize("Seller")]
		[HttpGet("Seller/{userId}/{productId}")]
		public IActionResult GetProductSeller(long userId, long productId)
		{
			try
			{
				if (userId != _jwtTokenService.GetUserIdByAccessToken(User))
				{
					return Unauthorized();
				}
				Product? product = _productRepository.GetProductByShop(userId, productId);
				if (product == null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "NOT FOUND", false, new()));
				}
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "SUCCESS", true, _mapper.Map<ProductResponseDTO>(product)));
			}
			catch (Exception)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_FAILD, "FAIL", false, new()));
			}

		}
		#endregion

		#region Add new product
		[Authorize("Seller")]
		[HttpPost("Add")]
		public async Task<IActionResult> AddProduct([FromForm] AddProductRequestDTO request)
		{
			if (!ModelState.IsValid)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "INVALID", false, new()));
			}
			try
			{
				string[] fileExtension = new string[] { ".jpge", ".png", ".jpg" };
				if(request.UserId != _jwtTokenService.GetUserIdByAccessToken(User))
				{
					return Unauthorized();
				}
				if (request.DataVariants.Any(x => !x.FileName.Contains(".xlsx"))
					||
					request.Images.Any(x => !fileExtension.Contains(x.FileName.Substring(x.FileName.LastIndexOf("."))))
					||
					!fileExtension.Contains(request.Thumbnail.FileName.Substring(request.Thumbnail.FileName.LastIndexOf("."))))
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "INVALID", false, new()));
				}
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
						AssetInformations = assetInformation,
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

				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "SUCCESS", true, new()));

			}
			catch (Exception e)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_FAILD, e.Message, false, new()));
			}
		}
		#endregion

		#region Edit product
		[Authorize("Seller")]
		[HttpPost("Edit")]
		public async Task<IActionResult> EditProduct([FromForm] EditProductRequestDTO request)
		{
			try
			{
				if(request.UserId != _jwtTokenService.GetUserIdByAccessToken(User))
				{
					return Unauthorized();
				}
				Product? prod = _productRepository.GetProductByShop(request.UserId, request.ProductId);
				if (prod == null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "NOT FOUND", false, new()));
				}

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
						AssetInformations = request.ProductVariantFileUpdate.Count == 0 || request.ProductVariantFileUpdate[i] == null ? null : Util.Instance.ReadDataFileExcelProductVariant(request.ProductVariantFileUpdate[i]),
					});
				}
				List<ProductVariant> productVariantsNew = new List<ProductVariant>();
				for (int i = 0; i < request.ProductVariantFileNew.Count; i++)
				{
					productVariantsNew.Add(new ProductVariant
					{
						AssetInformations = Util.Instance.ReadDataFileExcelProductVariant(request.ProductVariantFileNew[i]),
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
					Thumbnail = request.ProductThumbnail == null ? null : urlThumbnailNew,
					ProductStatusId = request.IsActiveProduct ? Constants.PRODUCT_ACTIVE : Constants.PRODUCT_HIDE
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
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "SUCCESS", true, new()));
			}
			catch (Exception e)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_FAILD, e.Message, false, new()));
			}
		}
		#endregion
	}
}
