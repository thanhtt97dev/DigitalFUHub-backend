using AutoMapper;
using Azure.Core;
using BusinessObject.Entities;
using Comons;
using DataAccess.IRepositories;
using DataAccess.Repositories;
using DigitalFUHubApi.Comons;
using DigitalFUHubApi.Services;
using DTOs.Feedback;
using DTOs.Product;
using DTOs.ReportProduct;
using DTOs.Seller;
using DTOs.Shop;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

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
		private readonly HubService _hubService;
		private readonly MailService _mailService;
		private readonly IMapper _mapper;

		public ProductsController(IConfiguration configuration,
			IProductRepository productRepository,
			StorageService storageService,
			IShopRepository shopRepository,
			IUserRepository userRepository,
			IRoleRepository roleRepository,
			IOrderRepository orderRepository,
			ICouponRepository couponRepository,
			JwtTokenService jwtTokenService,
			HubService hubService,
			MailService mailService,
			IMapper mapper)
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
			_mailService = mailService;
			_hubService = hubService;
		}

		#region Get Product Detail
		[HttpGet("GetById/{productId}")]
		public IActionResult GetById(long productId)
		{
			ResponseData responseData = new ResponseData();
			Status status = new Status();
			try
			{
				if (productId == 0)
				{
					status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
					status.Message = "Invalid";
					status.Ok = false;
					responseData.Status = status;
					return Ok(responseData);
				}

				var product = _productRepository.GetProductById(productId);

				if (product == null)
				{
					status.ResponseCode = Constants.RESPONSE_CODE_DATA_NOT_FOUND;
					status.Message = "Product not found!";
					status.Ok = false;
					responseData.Status = status;
					return Ok(responseData);
				}

				// check product status
				if (product.ProductStatusId == Constants.PRODUCT_STATUS_BAN)
				{
					status.ResponseCode = Constants.RESPONSE_CODE_PRODUCT_BAN;
					status.Message = "This product has been banned";
					status.Ok = false;
					responseData.Status = status;
					responseData.Result = product;
					return Ok(responseData);
				}

				if (product.ProductStatusId == Constants.PRODUCT_STATUS_REMOVE)
				{
					status.ResponseCode = Constants.RESPONSE_CODE_PRODUCT_REMOVE;
					status.Message = "This product has been remove";
					status.Ok = false;
					responseData.Status = status;
					return Ok(responseData);
				}

				if (product.ProductStatusId == Constants.PRODUCT_STATUS_HIDE)
				{
					status.ResponseCode = Constants.RESPONSE_CODE_PRODUCT_HIDE;
					status.Message = "This product has been hide";
					status.Ok = false;
					responseData.Status = status;
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

		#region Get list product of seller
		[Authorize("Seller")]
		[HttpGet("Seller/List")]
		public IActionResult GetListProductBySeller(string productId, string productName, int page)
		{
			try
			{
				if (page <= 0)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid page", false, new()));
				}
				long userId = _jwtTokenService.GetUserIdByAccessToken(User);
				(List<Product> products, long totalItems) = _productRepository.GetListProductOfSeller(userId, productId,
					productName, page);

				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, new SellerGetProductResponseDTO
				{
					Products = products,
					TotalItems = totalItems
				}));
			}
			catch (Exception e)
			{
				return BadRequest(e.Message);
			}

		}
		#endregion

		#region Get products of seller - HieuLD6
		[Authorize("Seller")]
		[HttpPost("getProducts")]
		public IActionResult GetProductsSeller(GetProductsOfSellerRequestDTO request)
		{
			if (request.UserId != _jwtTokenService.GetUserIdByAccessToken(User))
			{
				return Unauthorized();
			}
			if (!ModelState.IsValid)
			{
				return BadRequest();
			}
			try
			{
				if (request.SoldMin < 0 || request.SoldMax < 0 || request.Page <= 0 || !Constants.PRODUCT_STATUS.Contains(request.ProductStatusId) ||
					(request.SoldMin != 0 && request.SoldMax != 0 && (request.SoldMin > request.SoldMax))
					)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid params", false, new()));
				}

				var numberProducts = _productRepository.GetNumberProductByConditions(request.UserId, string.Empty, request.ProductId, request.ProductName, request.ProductCategory,
					 request.SoldMin, request.SoldMax, request.ProductStatusId);
				var numberPages = numberProducts / Constants.PAGE_SIZE + 1;

				if (request.Page > numberPages)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid number page", false, new()));
				}

				List<Product> products = _productRepository
					.GetProductsOfSeller(request.UserId, request.ProductId, request.ProductName, request.ProductCategory,
					 request.SoldMin, request.SoldMax, request.ProductStatusId, request.Page);

				var result = new GetProductsResponseDTO
				{
					TotalProduct = numberProducts,
					TotalPage = numberPages,
					Products = _mapper.Map<List<GetProductsProductDetailResponseDTO>>(products)
				};

				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "SUCCESS", true, result));
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
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
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Not found", false, new()));
				}
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, _mapper.Map<ProductResponseDTO>(product)));
			}
			catch (Exception e)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
			}

		}
		#endregion

		#region Add product
		[Authorize("Seller")]
		[HttpPost("Add")]
		public async Task<IActionResult> AddProduct([FromForm] AddProductRequestDTO request)
		{
			try
			{
				//if (request.UserId != _jwtTokenService.GetUserIdByAccessToken(User))
				//{
				//	return Unauthorized();
				//}
				if (!ModelState.IsValid
					|| string.IsNullOrWhiteSpace(request.ProductName)
					|| string.IsNullOrWhiteSpace(request.Description)
					)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid data", false, new()));
				}
				if (request.Tags == null || request.Tags.Count == 0)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid product tags", false, new()));
				}
				if (!(request.ProductVariantNames.Count() == request.ProductVariantPrices.Count()
					&& request.ProductVariantPrices.Count() == request.ProductVariantDiscounts.Count()
					&& request.ProductVariantDiscounts.Count() == request.AssetInformationFiles.Count()
					&& request.AssetInformationFiles.Count() == request.ProductVariantNames.Count()))
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid productvariant", false, new()));
				}
				if (request.ProductVariantNames.Any(x => string.IsNullOrWhiteSpace(x)))
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid productvariant name", false, new()));
				}
				if (request.ProductVariantDiscounts.Any(x => x < Constants.MIN_PERCENT_PRODUCT_VARIANT_DISCOUNT || x > Constants.MAX_PERCENT_PRODUCT_VARIANT_DISCOUNT))
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid productvariant discount", false, new()));
				}
				if (request.ProductVariantPrices.Any(x => x < Constants.MIN_PRICE_PRODUCT_VARIANT || x > Constants.MAX_PRICE_PRODUCT_VARIANT))
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid productvariant price", false, new()));
				}
				string[] fileExtension = new string[] { ".jpge", ".png", ".jpg" };
				// check file upload satisfy file extension
				if (request.AssetInformationFiles.Any(x => !x.FileName.Contains(".xlsx"))
					||
					request.ProductDetailImageFiles.Any(x => !fileExtension.Contains(x.FileName.Substring(x.FileName.LastIndexOf("."))))
					||
					!fileExtension.Contains(request.ThumbnailFile.FileName.Substring(request.ThumbnailFile.FileName.LastIndexOf("."))))
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid file", false, new()));
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
						Discount = request.ProductVariantDiscounts[i],
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
					Discount = 0,
					ProductName = request.ProductName,
					ShopId = request.UserId,
					Tags = tags,
					Thumbnail = urlThumbnail,
					ProductVariants = productVariants,
					ProductMedias = productMedias,
					ProductStatusId = Constants.PRODUCT_STATUS_ACTIVE,
					DateCreate = DateTime.Now,
					DateUpdate = DateTime.Now,
					TotalRatingStar = 0,
					NumberFeedback = 0,
				};

				_productRepository.AddProduct(product);

				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, new()));

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
				//if (request.UserId != _jwtTokenService.GetUserIdByAccessToken(User))
				//{
				//	return Unauthorized();
				//}
				if (request == null
					|| string.IsNullOrWhiteSpace(request.ProductName)
					|| string.IsNullOrWhiteSpace(request.ProductDescription)
					)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid data", false, new()));
				}
				if (request.Tags == null || request.Tags.Count == 0)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid product tags", false, new()));
				}
				if (!(request.ProductVariantIdsUpdate.Count() == request.ProductVariantNamesUpdate.Count()
					&& request.ProductVariantNamesUpdate.Count() == request.ProductVariantDiscountsUpdate.Count()
					&& request.ProductVariantDiscountsUpdate.Count() == request.ProductVariantPricesUpdate.Count()
					&& request.ProductVariantPricesUpdate.Count() == request.ProductVariantIdsUpdate.Count())
					||
					!(request.ProductVariantNamesAddNew.Count() == request.ProductVariantPricesAddNew.Count()
					&& request.ProductVariantPricesAddNew.Count() == request.ProductVariantDiscountsAddNew.Count()
					&& request.ProductVariantDiscountsAddNew.Count() == request.AssetInformationFilesAddNew.Count()
					&& request.AssetInformationFilesAddNew.Count() == request.ProductVariantNamesAddNew.Count()))
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid productvariant", false, new()));
				}
				if (request.ProductVariantNamesAddNew.Any(x => string.IsNullOrWhiteSpace(x)) ||
					request.ProductVariantNamesUpdate.Any(x => string.IsNullOrWhiteSpace(x)))
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid productvariant name", false, new()));
				}
				if (request.ProductVariantDiscountsUpdate.Any(x => x < Constants.MIN_PERCENT_PRODUCT_VARIANT_DISCOUNT || x > Constants.MAX_PERCENT_PRODUCT_VARIANT_DISCOUNT)
					|| request.ProductVariantDiscountsAddNew.Any(x => x < Constants.MIN_PERCENT_PRODUCT_VARIANT_DISCOUNT || x > Constants.MAX_PERCENT_PRODUCT_VARIANT_DISCOUNT))
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid productvariant discount", false, new()));

				}
				if (request.ProductVariantPricesUpdate.Any(x => x < Constants.MIN_PRICE_PRODUCT_VARIANT || x > Constants.MAX_PRICE_PRODUCT_VARIANT)
					|| request.ProductVariantPricesAddNew.Any(x => x < Constants.MIN_PRICE_PRODUCT_VARIANT || x > Constants.MAX_PRICE_PRODUCT_VARIANT))
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid productvariant price", false, new()));
				}
				Product? prod = _productRepository.CheckProductExist(request.UserId, request.ProductId);
				if (prod == null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Not found", false, new()));
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
							Discount = request.ProductVariantDiscountsUpdate[i],
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
							Discount = request.ProductVariantDiscountsAddNew[i],
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
					Discount = 0,
					CategoryId = request.CategoryId,
					Thumbnail = request.ProductThumbnailFileUpdate == null ? null : urlThumbnailNew,
					ProductStatusId = request.IsActiveProduct ? Constants.PRODUCT_STATUS_ACTIVE : Constants.PRODUCT_STATUS_HIDE
				};

				_productRepository.EditProduct(product, productVariantsAddNew, productVariantsUpdate,
					listTagAddNew, listProductDetailImagesAddNew, request.ProductDetailImagesCurrent);

				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, new()));
			}
			catch (Exception e)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
			}
		}
		#endregion

		#region Get products for admin
		[Authorize("Admin")]
		[HttpPost("admin/getProducts")]
		public IActionResult GetProductsAdmin(GetProductsRequestDTO request)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest();
			}
			try
			{
				if (request.SoldMin < 0 || request.SoldMax < 0 || request.Page <= 0 || !Constants.PRODUCT_STATUS.Contains(request.ProductStatusId) ||
					(request.SoldMin != 0 && request.SoldMax != 0 && (request.SoldMin > request.SoldMax))
					)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid params", false, new()));
				}

				var numberProducts = _productRepository.GetNumberProductByConditions(request.ShopId, request.ShopName, request.ProductId, request.ProductName, request.ProductCategory,
					 request.SoldMin, request.SoldMax, request.ProductStatusId);
				var numberPages = numberProducts / Constants.PAGE_SIZE + 1;

				if (request.Page > numberPages)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid number page", false, new()));
				}

				List<Product> products = _productRepository
					.GetProductsForAdmin(request.ShopId, request.ShopName, request.ProductId, request.ProductName, request.ProductCategory,
					 request.SoldMin, request.SoldMax, request.ProductStatusId, request.Page);

				var result = new GetProductsResponseDTO
				{
					TotalProduct = numberProducts,
					TotalPage = numberPages,
					Products = _mapper.Map<List<GetProductsProductDetailResponseDTO>>(products)
				};

				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "SUCCESS", true, result));
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Get product detail for admin
		[Authorize("Admin")]
		[HttpGet("admin/{id}")]
		public IActionResult GetProductDetailAdmin(long id)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest();
			}
			try
			{
				var product = _productRepository.GetProduct(id);
				if (product == null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Not found", false, new()));
				}
				var result = _mapper.Map<ProductDetailAdminResponseDTO>(product);
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "SUCCESS", true, result));
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Update product status for admin
		[Authorize("Admin")]
		[HttpPost("admin/update")]
		public async Task<IActionResult> GetProductDetailAdmin(UpdateProductStatusAdminRequestDTO request)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest();
			}
			try
			{
				var product = _productRepository.GetProduct(request.ProductId);
				if (product == null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Not found", false, new()));
				}
				_productRepository.UpdateProductStatusAdmin(request.ProductId, request.Status, request.Note);
				if (request.Status == Constants.PRODUCT_STATUS_BAN)
				{
					// send notification
					await _hubService.SendNotification(product.Shop.UserId, "Sản phẩm đã bị cấm", $"Sản phẩm mã số #{product.ProductId} đã bị cấm", Constants.FRONT_END_SELLER_PRODUCT_URL + "list");
					// send mail
					await _mailService.SendEmailAsync(product.Shop.User.Email, $"DigitalFuHub: Sản phẩm mã số #{product.ProductId} đã bị cấm", "Bạn vui lòng truy cập vào webside của chúng tôi để kiểm tra thêm thông tin");
				}
				else
				{
					// send notification
					await _hubService.SendNotification(product.Shop.UserId, "Sản phẩm của bạn đã được kích hoạt", $"Sản phẩm mã số #{product.ProductId} đã hoạt động trở lại", Constants.FRONT_END_SELLER_PRODUCT_URL + product.ProductId.ToString());
				}
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "SUCCESS", true, new()));

			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}

		}
		#endregion


		#region Get Products (Shop Detail Customer)
		[HttpPost("GetAll")]
		public IActionResult GetProductByUserId(ShopDetailCustomerSearchParamProductRequestDTO request)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest();
			}
			try
			{
				ResponseData responseData = new ResponseData();
				Status status = new Status();
				if (request.Page <= 0)
				{
					status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
					status.Message = "Invalid param";
					status.Ok = false;
					responseData.Status = status;
					return Ok(responseData);
				}

				var numberProducts = _productRepository.GetNumberProductByConditions(request.UserId, request.ProductName);
				var numberPages = numberProducts / Constants.PAGE_SIZE_PRODUCT + 1;
				if (request.Page > numberPages)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid number page", false, new()));
				}

				List<Product> products = _productRepository.GetProductByUserId(request.UserId, request.Page, request.ProductName);
				List<ShopDetailCustomerProductDetailResponseDTO> productResponses = _mapper.Map<List<ShopDetailCustomerProductDetailResponseDTO>>(products);

				var result = new ShopDetailCustomerProductResponseDTO
				{
					TotalProduct = numberProducts,
					TotalPage = numberPages,
					Products = productResponses
				};

				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "SUCCESS", true, result));
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion


		#region Get Products (Home Page Customer)
		[HttpPost("GetProductHomePageCustomer")]
		public IActionResult GetProductForHomePageCustomer(HomePageCustomerSearchParamProductRequestDTO request)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest();
			}
			try
			{
				ResponseData responseData = new ResponseData();
				Status status = new Status();
				if (request.Page <= 0)
				{
					status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
					status.Message = "Invalid param";
					status.Ok = false;
					responseData.Status = status;
					return Ok(responseData);
				}

				var numberProducts = _productRepository.GetNumberProductByConditions(request.CategoryId);
				var numberPages = numberProducts / Constants.PAGE_SIZE_PRODUCT_HOME_PAGE + 1;
				if (request.Page > numberPages)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid number page", false, new()));
				}

				List<Product> products = _productRepository.GetProductForHomePageCustomer(request.Page, request.CategoryId, request.IsOrderFeedback, request.IsOrderSoldCount);

				List<HomePageCustomerProductDetailResponseDTO> productResponses = _mapper.Map<List<HomePageCustomerProductDetailResponseDTO>>(products);

				var result = new HomePageCustomerProductResponseDTO
				{
					TotalProduct = numberProducts,
					Products = productResponses
				};

				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "SUCCESS", true, result));
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Search Product
		[HttpPost("Search")]
		public IActionResult SearchProduct(SearchProductRequestDTO request)
		{
			if(string.IsNullOrWhiteSpace(request.Keyword))
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid keyword search", false, new()));
			}

			if (request.MinPrice >= request.MaxPrice)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid range price", false, new()));
			}
			if (request.Page <= 0)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid page", false, new()));
			}
			(long totalItems, List<Product> products) = _productRepository.GetListProductSearched(request.Keyword, 
				request.Category, request.Rating ,request.MinPrice, request.MaxPrice, request.Sort, request.Page);
			return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, new SearchProductResponseDTO
			{
				TotalItems = totalItems,
				Products = _mapper.Map<List<HomePageCustomerProductDetailResponseDTO>>(products)
			}));
		}
		#endregion
	}
}
