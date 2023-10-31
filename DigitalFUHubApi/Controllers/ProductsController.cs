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

        #region Get Product Detail
        [HttpGet("GetById/{productId}")]
		public IActionResult GetById(long productId)
		{
            ResponseData responseData = new ResponseData();
            Status status = new Status();
            try
			{
				if (productId == 0) {
					status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
					status.Message = "Invalid";
					status.Ok = false;
					responseData.Status = status;
					return Ok(responseData);
                }
				
                (var product, long productStatusId) = _productRepository.GetProductById(productId);

				if (product == null) {
                    status.ResponseCode = Constants.RESPONSE_CODE_DATA_NOT_FOUND;
                    status.Message = "Product not found!";
                    status.Ok = false;
                    responseData.Status = status;
                    return Ok(responseData);
                }

				// check product status
				if (productStatusId == Constants.PRODUCT_BAN)
				{
                    status.ResponseCode = Constants.RESPONSE_CODE_PRODUCT_BAN;
                    status.Message = "This product has been banned";
                    status.Ok = false;
                    responseData.Status = status;
                    responseData.Result = product;
                    return Ok(responseData);
                }

                if (productStatusId == Constants.PRODUCT_REMOVE)
                {
                    status.ResponseCode = Constants.RESPONSE_CODE_PRODUCT_REMOVE;
                    status.Message = "This product has been remove";
                    status.Ok = false;
                    responseData.Status = status;
                    return Ok(responseData);
                }

                if (productStatusId == Constants.PRODUCT_HIDE)
                {
                    status.ResponseCode = Constants.RESPONSE_CODE_PRODUCT_HIDE;
                    status.Message = "This product has been hide";
                    status.Ok = false;
                    responseData.Status = status;
                    responseData.Result = product;
                    return Ok(responseData);
                }

                status.ResponseCode = Constants.RESPONSE_CODE_PRODUCT_ACTIVE;
                status.Message = "Success";
                status.Ok = true;
                responseData.Status = status;
                responseData.Result = product;
                return Ok(responseData);

            }
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
        #endregion

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
			catch (Exception e)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
			}

		}
		#endregion

		#region Add new product
		[Authorize("Seller")]
		[HttpPost("Add")]
		public async Task<IActionResult> AddProduct([FromForm] AddProductRequestDTO request)
		{
			try
			{
				if (request.UserId != _jwtTokenService.GetUserIdByAccessToken(User))
				{
					return Unauthorized();
				}
				if (!ModelState.IsValid)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "INVALID", false, new()));
				}
				string[] fileExtension = new string[] { ".jpge", ".png", ".jpg" };
				// check file upload satisfy file extension
				if (request.AssetInformationFiles.Any(x => !x.FileName.Contains(".xlsx"))
					||
					request.ProductDetailImageFiles.Any(x => !fileExtension.Contains(x.FileName.Substring(x.FileName.LastIndexOf("."))))
					||
					!fileExtension.Contains(request.ThumbnailFile.FileName.Substring(request.ThumbnailFile.FileName.LastIndexOf("."))))
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "INVALID FILE", false, new()));
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
				for (int i = 0; i < request.AssetInformationFiles.Count; i++)
				{
					List<AssetInformation> assetInformation = Util.Instance.ReadDataFileExcelProductVariant(request.AssetInformationFiles[i]);

					productVariants.Add(new ProductVariant
					{
						Name = request.ProductVariantNames[i],
						Price = request.ProductVariantPrices[i],
						AssetInformations = assetInformation,
						isActivate = true,
					});
				}
				List<ProductMedia> productMedias = new List<ProductMedia>();
				// upload product detail image file to azure
				foreach (IFormFile file in request.ProductDetailImageFiles)
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
				filename = string.Format("{0}_{1}{2}{3}{4}{5}{6}{7}{8}", request.UserId, now.Year, now.Month, now.Day,
					now.Millisecond, now.Second, now.Minute, now.Hour,
					request.ThumbnailFile.FileName.Substring(request.ThumbnailFile.FileName.LastIndexOf(".")));
				string urlThumbnail = await _storageService.UploadFileToAzureAsync(request.ThumbnailFile, filename);

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
				return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
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
				if (request.UserId != _jwtTokenService.GetUserIdByAccessToken(User))
				{
					return Unauthorized();
				}
				if (request == null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "INVALID", false, new()));
				}
				Product? prod = _productRepository.CheckProductExist(request.UserId, request.ProductId);
				if (prod == null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "NOT FOUND", false, new()));
				}

				string filename = "";
				DateTime now;

				List<Tag> listTagAddNew = new List<Tag>();
				request.Tags.ForEach((value) =>
				{
					listTagAddNew.Add(new Tag { ProductId = request.ProductId, TagName = value });
				});

				List<ProductMedia> listProductDetailImagesAddNew = new List<ProductMedia>();
				// Check if there are new product detail images add new or not 
				if (request.ProductDetailImagesAddNew.Count > 0)
				{
					foreach (var file in request.ProductDetailImagesAddNew)
					{
						now = DateTime.Now;
						filename = string.Format("{0}_{1}{2}{3}{4}{5}{6}{7}{8}", request.UserId, now.Year, now.Month, now.Day,
							now.Millisecond, now.Second, now.Minute, now.Hour, file.FileName.Substring(file.FileName.LastIndexOf(".")));
						// upload to azure
						string url = await _storageService.UploadFileToAzureAsync(file, filename);
						listProductDetailImagesAddNew.Add(new ProductMedia
						{
							ProductId = request.ProductId,
							Url = url,
						});
					};
				}

				List<ProductVariant> productVariantsUpdate = new List<ProductVariant>();
				// check if there are product variant update (not include delete)
				if (request.ProductVariantIdsUpdate.Count > 0)
				{
					for (int i = 0; i < request.ProductVariantIdsUpdate.Count; i++)
					{
						#pragma warning disable CS8601 // Possible null reference assignment.
						productVariantsUpdate.Add(new ProductVariant
						{
							Name = request.ProductVariantNamesUpdate[i],
							Price = request.ProductVariantPricesUpdate[i],
							ProductId = request.ProductId,
							ProductVariantId = request.ProductVariantIdsUpdate[i],
							AssetInformations = request.AssetInformationFilesUpdate != null && request.AssetInformationFilesUpdate.Count > 0
												&& request.AssetInformationFilesUpdate[i] != null ?
									Util.Instance.ReadDataFileExcelProductVariant(request.AssetInformationFilesUpdate[i]) : null
						});
						#pragma warning restore CS8601 // Possible null reference assignment.
					}
				}

				List<ProductVariant> productVariantsAddNew = new List<ProductVariant>();
				// check add new product variant or not
				if (request.ProductVariantNamesAddNew.Count > 0)
				{
					for (int i = 0; i < request.ProductVariantNamesAddNew.Count; i++)
					{
						productVariantsAddNew.Add(new ProductVariant
						{
							AssetInformations = Util.Instance.ReadDataFileExcelProductVariant(request.AssetInformationFilesAddNew[i]),
							isActivate = true,
							Name = request.ProductVariantNamesAddNew[i],
							Price = request.ProductVariantPricesAddNew[i],
							ProductId = request.ProductId,
						});
					}
				}


				// check product detail image is delete or not
				if (prod.ProductMedias.Count(x => !request.ProductDetailImagesCurrent.Any(m => m == x.Url)) > 0)
				{
					List<ProductMedia> productDetailImagesDelete = prod.ProductMedias
						.Where(x => !request.ProductDetailImagesCurrent.Any(m => m == x.Url)).ToList();
					foreach (ProductMedia image in productDetailImagesDelete)
					{
						await _storageService.RemoveFileFromAzureAsync(image.Url.Substring(image.Url.LastIndexOf("/") + 1));
					}
				}

				string urlThumbnailNew = "";

				// check update product thumbnail or not
				if (request.ProductThumbnailFileUpdate != null)
				{
					now = DateTime.Now;
					filename = string.Format("{0}_{1}{2}{3}{4}{5}{6}{7}{8}", request.UserId, now.Year, now.Month,
						now.Day, now.Millisecond, now.Second, now.Minute, now.Hour,
						request.ProductThumbnailFileUpdate.FileName.Substring(request.ProductThumbnailFileUpdate.FileName.LastIndexOf(".")));
					urlThumbnailNew = await _storageService.UploadFileToAzureAsync(request.ProductThumbnailFileUpdate, filename);

					// delete product thumbnail old
#pragma warning disable CS8602 // Dereference of a possibly null reference.
					await _storageService.RemoveFileFromAzureAsync(prod.Thumbnail.Substring(prod.Thumbnail.LastIndexOf("/") + 1));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
				}

				Product product = new Product
				{
					ProductId = request.ProductId,
					ProductName = request.ProductName,
					Description = request.ProductDescription,
					Discount = request.Discount,
					CategoryId = request.CategoryId,
					Thumbnail = request.ProductThumbnailFileUpdate == null ? null : urlThumbnailNew,
					ProductStatusId = request.IsActiveProduct ? Constants.PRODUCT_ACTIVE : Constants.PRODUCT_HIDE
				};

				_productRepository.EditProduct(product, productVariantsAddNew, productVariantsUpdate,
					listTagAddNew, listProductDetailImagesAddNew, request.ProductDetailImagesCurrent);

				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "SUCCESS", true, new()));
			}
			catch (Exception e)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
			}
		}
		#endregion
	}
}
