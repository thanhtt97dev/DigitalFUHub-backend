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

namespace DigitalFUHubApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ShopsController : ControllerBase
	{
		private readonly IShopRepository _shopRepository;
		private readonly IUserRepository _userRepository;
		private readonly JwtTokenService _jwtTokenService;
		private readonly StorageService _storageService;
		private readonly IMapper _mapper;

		public ShopsController(IShopRepository shopRepository, 
			IUserRepository userRepository, 
			JwtTokenService jwtTokenService, 
			StorageService storageService,
			IMapper mapper)
		{
			_shopRepository = shopRepository;
			_userRepository = userRepository;
			_jwtTokenService = jwtTokenService;
			_storageService = storageService;
			_mapper = mapper;
		}

		[Authorize]
		[HttpGet("IsExistShopName/{shopName}")]
		public IActionResult CheckExistShopName(string shopName)
		{
			if (string.IsNullOrWhiteSpace(shopName))
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "INVALID", false, new()));
			}
			bool result = _shopRepository.IsExistShopName(shopName.Trim());
			return Ok(new ResponseData(!result ? Constants.RESPONSE_CODE_SUCCESS : Constants.RESPONSE_CODE_NOT_ACCEPT, !result ? "SUCCESS" : "INVALID", !result, new()));
		}
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
			return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, shop));

		}
		#endregion

		#region register seller
		[Authorize("Customer")]
		[HttpPost("Register")]
		public async Task<IActionResult> Register([FromForm] RegisterShopRequestDTO request)
		{
			string[] fileExtension = new string[] { ".jpge", ".png", ".jpg" };
			if (!ModelState.IsValid
				|| string.IsNullOrWhiteSpace(request.ShopName)
				|| string.IsNullOrWhiteSpace(request.ShopDescription)
				|| !fileExtension.Contains(request.AvatarFile.FileName.Substring(request.AvatarFile.FileName.LastIndexOf("."))))
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "INVALID", false, new()));
			}
			try
			{
				if (request.UserId != _jwtTokenService.GetUserIdByAccessToken(User))
				{
					return Unauthorized();
				}
				DateTime now = DateTime.Now;
				string filename = string.Format("{0}_{1}{2}{3}{4}{5}{6}{7}{8}", request.UserId, now.Year, now.Month, now.Day,
					now.Millisecond, now.Second, now.Minute, now.Hour,
					request.AvatarFile.FileName.Substring(request.AvatarFile.FileName.LastIndexOf(".")));
				string avatarUrl = await _storageService.UploadFileToAzureAsync(request.AvatarFile, filename);
				_shopRepository.AddShop(avatarUrl, request.ShopName.Trim(), request.UserId, request.ShopDescription.Trim());
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "SUCCESS", true, new()));
			}
			catch (Exception e)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
			}
		}
		#endregion

		#region register seller
		[Authorize("Seller")]
		[HttpPost("Seller/Edit")]
		public async Task<IActionResult> EditShop([FromForm]SellerEditShopRequestDTO request)
		{
			string[] fileExtension = new string[] { ".jpge", ".png", ".jpg" };
			if (string.IsNullOrWhiteSpace(request.ShopDescription)
				|| (request.AvatarFile != null &&
				!fileExtension.Contains(request.AvatarFile.FileName.Substring(request.AvatarFile.FileName.LastIndexOf(".")))))
			{
				return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "INVALID", false, new()));
			}
			try
			{
				string avatarUrl = "";
				if (request.AvatarFile != null)
				{
					DateTime now = DateTime.Now;
					string filename = string.Format("{0}_{1}{2}{3}{4}{5}{6}{7}{8}", request.UserId, now.Year, now.Month, now.Day,
						now.Millisecond, now.Second, now.Minute, now.Hour,
						request.AvatarFile.FileName.Substring(request.AvatarFile.FileName.LastIndexOf(".")));
					 avatarUrl = await _storageService.UploadFileToAzureAsync(request.AvatarFile, filename);
				}
				Shop shop = new Shop
				{
					Avatar = avatarUrl,
					UserId = _jwtTokenService.GetUserIdByAccessToken(User),
					DateCreate = DateTime.Now,
					Description = request.ShopDescription
				};
				_shopRepository.EditShop(shop);
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, new()));
			}
			catch (Exception e)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
			}
		}
		#endregion

		#region Get shops for admin
		//[Authorize("Admin")]
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

        #region Get info shop
        //[Authorize]
        [HttpPost("GetDetail/{userId}")]
        public IActionResult GetShopDetail(long userId)
        {
            ResponseData responseData = new ResponseData();
            Status status = new Status();
            try
            {
                if (userId == 0)
                {
                    status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
                    status.Ok = false;
                    status.Message = "user id invalid!";
                    responseData.Status = status;
                    return Ok(responseData);
                }

                if (userId != _jwtTokenService.GetUserIdByAccessToken(User))
                {
                    return Unauthorized();
                }

                var user = _userRepository.GetUserById(userId);

                if (user == null)
                {
                    status.ResponseCode = Constants.RESPONSE_CODE_DATA_NOT_FOUND;
                    status.Ok = false;
                    status.Message = "user not found!";
                    responseData.Status = status;
                    return Ok(responseData);
                }

				Shop? shop = _shopRepository.GetShopById(userId);

				if (shop == null)
				{
                    status.ResponseCode = Constants.RESPONSE_CODE_DATA_NOT_FOUND;
                    status.Ok = false;
                    status.Message = "shop not found!";
                    responseData.Status = status;
                    return Ok(responseData);
                }

				var result = _mapper.Map<List<GetShopsResponseDTO>>(shop);

				// Ok
				status.ResponseCode = Constants.RESPONSE_CODE_SUCCESS;
                status.Ok = false;
                status.Message = "Success";
                responseData.Status = status;
				responseData.Result = result;
                return Ok(responseData);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
        #endregion

    }
}
