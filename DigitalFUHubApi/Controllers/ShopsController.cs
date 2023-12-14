using AutoMapper;
using BusinessObject.Entities;
using Comons;
using DataAccess.IRepositories;
using DataAccess.Repositories;
using DigitalFUHubApi.Comons;
using DigitalFUHubApi.Services;
using DTOs.Product;
using DTOs.Seller;
using DTOs.Shop;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.UserSecrets;
using System.Text.RegularExpressions;

namespace DigitalFUHubApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ShopsController : ControllerBase
	{
		private readonly IShopRepository _shopRepository;
		private readonly IUserRepository _userRepository;
		private readonly JwtTokenService _jwtTokenService;
		private readonly AzureStorageAccountService _azureStorageAccountService;
		private readonly MailService _mailService;

		private readonly IMapper _mapper;

		public ShopsController(IShopRepository shopRepository, 
			IUserRepository userRepository, 
			JwtTokenService jwtTokenService, 
			AzureStorageAccountService azureStorageAccountService, 
			MailService mailService, 
			IMapper mapper)
		{
			_shopRepository = shopRepository;
			_userRepository = userRepository;
			_jwtTokenService = jwtTokenService;
			_azureStorageAccountService = azureStorageAccountService;
			_mailService = mailService;
			_mapper = mapper;
		}



		#region check exist shop name
		[Authorize]
		[HttpGet("IsExistShopName/{shopName}")]
		public IActionResult CheckExistShopName(string shopName)
		{
			if (string.IsNullOrWhiteSpace(shopName))
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid", false, new()));
			}
			bool result = _shopRepository.IsExistShopName(shopName.Trim());
			return Ok(new ResponseData(!result ? Constants.RESPONSE_CODE_SUCCESS : Constants.RESPONSE_CODE_NOT_ACCEPT, !result ? "Success" : "Invalid", !result, new()));
		}
		#endregion

		#region check user has shop
		[Authorize("Customer,Seller")]
		[HttpGet("Exist")]
		public IActionResult CheckExistShop()
		{
			Shop? shop = _shopRepository.GetShopById(_jwtTokenService.GetUserIdByAccessToken(User));
			if (shop == null)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Not Found", false, new()));
			}
			if (!shop.IsActive)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SHOP_BANNED, "Shop banned", false, new()));
			}
			return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, new())) ;

		}
		#endregion

		#region get info shop of seller
		[Authorize("Seller")]
		[HttpGet("Seller/Get")]
		public IActionResult GetShopOfSeller()
		{
			Shop? shop = _shopRepository.GetShopById(_jwtTokenService.GetUserIdByAccessToken(User));
			if (shop == null)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Not Found", false, new()));
			}
			if (!shop.IsActive)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SHOP_BANNED, "Shop banned", false, new()));
			}
			return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, shop));

		}
		#endregion

		#region register seller
		[Authorize("Customer")]
		[HttpPost("Register")]
		public async Task<IActionResult> Register([FromForm] RegisterShopRequestDTO request)
		{
			try
			{
				string[] fileExtension = new string[] { ".jpge", ".png", ".jpg" };
				if (!ModelState.IsValid
					|| string.IsNullOrWhiteSpace(request.ShopName)
					|| string.IsNullOrWhiteSpace(request.ShopDescription)
					|| !fileExtension.Contains(request.AvatarFile.FileName.Substring(request.AvatarFile.FileName.LastIndexOf("."))))
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid data", false, new()));
				}
				User? user = _userRepository.GetUserById(request.UserId);
				if(user == null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Not found user", false, new()));
				}

				if (request.AvatarFile.Length > Constants.UPLOAD_FILE_SIZE_LIMIT)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Size file upload exceed 2MB", false, new()));
				}

				DateTime now = DateTime.Now;
				string filename = string.Format("{0}_{1}{2}{3}{4}{5}{6}{7}{8}", request.UserId, now.Year, now.Month, now.Day,
					now.Millisecond, now.Second, now.Minute, now.Hour,
					request.AvatarFile.FileName.Substring(request.AvatarFile.FileName.LastIndexOf(".")));
				string avatarUrl = await _azureStorageAccountService.UploadFileToAzureAsync(request.AvatarFile, filename);
				_shopRepository.AddShop(avatarUrl, request.ShopName.Trim(), request.UserId, request.ShopDescription.Trim());
				string html = $"<div>" +
					$"<h3>Xin chào {user.Fullname},</h3>" +
					$"<div>Xin chúc mừng bạn đã đăng ký bán hàng thành công.</div>" +
					$"<div>Tên cửa hàng của bạn là: <b>{request.ShopName.Trim()}</b></div>" +
					$" <div>" +
					$"<b>Vui lòng tuân thủ các quy định sau:</b>" +
					$"<div>" +
					$"<ul>" +
					$"<li>Tuân thủ các quy định về bảo mật thông tin cá nhân người mua</li>" +
					$"<li>Yêu cầu người bán xác nhận và xử lý các giao dịch một cách an toàn và đáng tin cậy</li>" +
					$"<li>Đảm bảo rằng mọi thông tin sản phẩm là chính xác và không gây lừa dối khách hàng</li>" +
					$"</ul>" +
					$"</div>" +
					$"</div>" +
					$"<b>Mọi thông tin thắc mắc xin vui lòng liên hệ: digitalfuhub@gmail.com</b>" +
					$"</div>";
				await _mailService.SendEmailAsync(user.Email, "DigitalFUHub: Đăng ký bán hàng thành công.", html);
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, new()));
			}
			catch (Exception e)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
			}
		}
		#endregion

		#region edit shop
		[Authorize("Seller")]
		[HttpPost("Seller/Edit")]
		public async Task<IActionResult> EditShop([FromForm] SellerEditShopRequestDTO request)
		{
			try
			{
				Shop? shop = _shopRepository.GetShopById(_jwtTokenService.GetUserIdByAccessToken(User));
				if (shop == null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Not found shop", false, new()));
				}
				if (!shop.IsActive)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_SHOP_BANNED, "Shop banned", false, new()));
				}
				string[] fileExtension = new string[] { ".jpge", ".png", ".jpg" };
				if (string.IsNullOrWhiteSpace(request.ShopDescription)
					|| (request.AvatarFile != null &&
					!fileExtension.Contains(request.AvatarFile.FileName.Substring(request.AvatarFile.FileName.LastIndexOf(".")))))
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid data", false, new()));
				}
				if (request.AvatarFile != null && request.AvatarFile.Length > Constants.UPLOAD_FILE_SIZE_LIMIT)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Size file upload exceed 2MB", false, new()));
				}
				string avatarUrl = "";
				if (request.AvatarFile != null)
				{
					DateTime now = DateTime.Now;
					string filename = string.Format("{0}_{1}{2}{3}{4}{5}{6}{7}{8}", request.UserId, now.Year, now.Month, now.Day,
						now.Millisecond, now.Second, now.Minute, now.Hour,
						request.AvatarFile.FileName.Substring(request.AvatarFile.FileName.LastIndexOf(".")));
					avatarUrl = await _azureStorageAccountService.UploadFileToAzureAsync(request.AvatarFile, filename);
				}
				Shop shopEdit = new Shop
				{
					Avatar = avatarUrl,
					UserId = _jwtTokenService.GetUserIdByAccessToken(User),
					DateCreate = DateTime.Now,
					Description = request.ShopDescription
				};
				_shopRepository.EditShop(shopEdit);
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, new()));
			}
			catch (Exception e)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
			}
		}
		#endregion

		#region Get shops for admin
		[Authorize("Admin")]
		[HttpPost("admin/all")]
		public IActionResult GetProductsSeller(GetShopsRequestDTO request)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest();
			}
			try
			{
				if (request.Page <= 0)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid number page", false, new()));
				}

				var numberShops = _shopRepository.GetNumberShopWithCondition(request.ShopId, request.ShopEmail, request.ShopName, request.ShopStatusId);
				var numberPages = numberShops / Constants.PAGE_SIZE + 1;

				if (request.Page > numberPages)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid number page", false, new()));
				}

				List<Shop> shops = _shopRepository.GetShopsWithCondition(request.ShopId, request.ShopEmail, request.ShopName, request.ShopStatusId, request.Page);

				var result = _mapper.Map<List<GetShopsResponseDTO>>(shops);

				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "SUCCESS", true, result));
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Get shop detail (customer)
		[HttpGet("getDetail/{userId}")]
		public IActionResult GetShopDetail(long userId)
		{
			try
			{
				Shop? shop = _shopRepository.GetShopDetail(userId);

				if (shop == null)
				{
                    return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Shop not found!", false, new()));
                }

				var shopResponse = _mapper.Map<ShopDetailCustomerResponseDTO>(shop);

                // Ok
                return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "SUCCESS", true, shopResponse));
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
        #endregion

        #region Get shop detail (admin)
        [HttpGet("admin/getById/{userId}")]
		[Authorize("Admin")]
		public IActionResult GetShopDetailAdmin(long userId)
        {
            try
            {
                Shop? shop = _shopRepository.GetShopDetailAdmin(userId);

                if (shop == null)
                {
                    return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Shop not found!", false, new()));
                }

                var shopResponse = _mapper.Map<ShopDetailAdminResponseDTO>(shop);

                // Ok
                return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "SUCCESS", true, shopResponse));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        #endregion

        #region Get most popular shop 
        [HttpGet("MostPopular")]
		public IActionResult GetMostPopularShop(string keyword)
		{
			if (string.IsNullOrWhiteSpace(keyword))
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid keyword search", false, new()));
			}
			Shop? shop = _shopRepository.GetMostPopularShop(keyword);
			if (shop == null)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Not found", false, new()));
			}
			ShopDetailCustomerResponseDTO response = _mapper.Map<ShopDetailCustomerResponseDTO>(shop);
			return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, response));
		}
		#endregion

		#region Get shop by search
		[HttpGet("Search")]
		public IActionResult GetShopsSearched(string keyword, int page)
		{
			if (string.IsNullOrWhiteSpace(keyword))
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid keyword search", false, new()));
			}
			if (page <= 0)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid page", false, new()));
			}
			(long totalItems, List<Shop> listShop) = _shopRepository.GetListShop(keyword, page);
			if (listShop.Count == 0)
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Not found", false, new()));
			}
			List<ShopDetailCustomerResponseDTO> response = _mapper.Map<List<ShopDetailCustomerResponseDTO>>(listShop);
			return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, new SearchShopResponseDTO
			{
				Shops = response,
				TotalItems = totalItems
			}));
		}
        #endregion

        #region Update shop (admin)
        [HttpPost("admin/update")]
        [Authorize("Admin")]
        public IActionResult UpdateShop(ShopDetailAdminUpdateStatusRequestDTO request)
        {
            try
            {
                Shop? shop = _shopRepository.GetShopById(request.ShopId);

                if (shop == null)
                {
                    return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Shop not found!", false, new()));
                }

				// update shop
				shop.IsActive = request.IsActive;
				shop.Note = request.Note;
				shop.DateBan = DateTime.Now;

				_shopRepository.UpdateShop(shop);

                // Ok
                return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "SUCCESS", true, new ()));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        #endregion

    }
}
