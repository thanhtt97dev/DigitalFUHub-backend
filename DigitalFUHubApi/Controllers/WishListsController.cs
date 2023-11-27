using AutoMapper;
using Azure.Core;
using BusinessObject.Entities;
using Comons;
using DataAccess.IRepositories;
using DataAccess.Repositories;
using DigitalFUHubApi.Comons;
using DigitalFUHubApi.Services;
using DTOs.Product;
using DTOs.WishList;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DigitalFUHubApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WishListsController : ControllerBase
    {

        private readonly JwtTokenService jwtTokenService;
        private readonly IUserRepository userRepository;
        private readonly IWishListRepository wishListRepository;
        private readonly IProductRepository productRepository;
        private readonly IMapper mapper;

        public WishListsController(IUserRepository userRepository, IProductRepository productRepository, JwtTokenService jwtTokenService, IWishListRepository wishListRepository, IMapper mapper) {
            this.jwtTokenService = jwtTokenService;
            this.userRepository = userRepository;
            this.wishListRepository = wishListRepository;
            this.productRepository = productRepository;
            this.mapper = mapper;
        }

		#region Add
		[HttpPost("Add")]
		[Authorize]
		public IActionResult AddWishList(WishListCustomerAddRequestDTO request)
		{
			try
			{
				if (request.UserId != jwtTokenService.GetUserIdByAccessToken(User))
				{
					return Unauthorized();
				}

				(string responseCode, string message, bool isOk) = wishListRepository.CheckRequestWishListIsValid(request.ProductId, request.UserId);
				if (!isOk)
				{
					return Ok(new ResponseData(responseCode, message, isOk, new()));
				}

				var product = productRepository.GetProductEntityById(request.ProductId);

                if (product == null)
                {
                    return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Product not found!", false, new()));
                }

					// check product status
				if (product.ProductStatusId == Constants.PRODUCT_STATUS_BAN)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_PRODUCT_BAN, "This product has been banned", false, new()));
				}

				if (product.ProductStatusId == Constants.PRODUCT_STATUS_REMOVE)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_PRODUCT_REMOVE, "This product has been remove", false, new()));
				}

				if (product.ProductStatusId == Constants.PRODUCT_STATUS_HIDE)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_PRODUCT_HIDE, "This product has been hide", false, new()));
				}

				bool isWishList = wishListRepository.IsExistWishList(request.ProductId, request.UserId);
				if (isWishList)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_PRODUCT_HIDE, "Wish list already exists", false, new()));
				}

				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, new()));
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Remove
		[HttpPost("Remove")]
		[Authorize]
		public IActionResult RemoveWishList(WishListCustomerRemoveRequestDTO request)
		{
			try
			{
				if (request.UserId != jwtTokenService.GetUserIdByAccessToken(User))
				{
					return Unauthorized();
				}

				(string responseCode, string message, bool isOk) = wishListRepository.CheckRequestWishListIsValid(request.ProductId, request.UserId);
				if (!isOk)
				{
					return Ok(new ResponseData(responseCode, message, isOk, new()));
				}

				var product = productRepository.GetProductEntityById(request.ProductId);
				if (product == null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Product not found!", false, new()));
				}

				bool isWishList = wishListRepository.IsExistWishList(request.ProductId, request.UserId);
				if (!isWishList)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Wish list does not exist", false, new()));
				}

				wishListRepository.RemoveWishList(request.ProductId, request.UserId);
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, new()));

			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Check wishlist existed
		[HttpGet("IsWishList")]
		[Authorize]
		public IActionResult CheckIsWistList(long productId, long userId)
		{
			try
			{
				if (userId != jwtTokenService.GetUserIdByAccessToken(User))
				{
					return Unauthorized();
				}

				(string responseCode, string message, bool isOk) = wishListRepository.CheckRequestWishListIsValid(productId, userId);
				if (!isOk)
				{
					return Ok(new ResponseData(responseCode, message, isOk, new()));
				}

				var product = productRepository.GetProductEntityById(productId);

				if (product == null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Product not found!", false, new()));
				}

				bool isWishList = wishListRepository.IsExistWishList(productId, userId);
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success", true, isWishList));

			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

		#region Get all with list of a user
		[HttpPost("GetAll")]
		[Authorize]
		public IActionResult GetWishListByUserId(WishListCustomerParamRequestDTO request)
		{
			try
			{
				if (request.UserId != jwtTokenService.GetUserIdByAccessToken(User)) return Unauthorized();

				var user = userRepository.GetUserById(request.UserId);
				if (user == null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "User not found!", false, new()));
				}

				if (request.Page <= 0)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid param!", false, new()));
				}

				var numberProducts = wishListRepository.GetNumberProductByConditions(request.UserId);
				var numberPages = numberProducts / Constants.PAGE_SIZE_PRODUCT_WISH_LIST + 1;
				if (request.Page > numberPages)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid number page", false, new()));
				}

				var products = wishListRepository.GetProductFromWishListByUserId(request.UserId, request.Page);

				List<WishListCustomerProductDetailResponseDTO> productResponses = mapper.Map<List<WishListCustomerProductDetailResponseDTO>>(products);

				var result = new WishListCustomerProductResponseDTO
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

		#region Remove list wishlist
		[HttpPost("RemoveSelecteds")]
		[Authorize]
		public IActionResult RemoveWishListSelecteds(WishListCustomertRemoveSelectedsRequestDTO request)
		{
			ResponseData responseData = new ResponseData();
			Status status = new Status();
			try
			{
				if (request.UserId != jwtTokenService.GetUserIdByAccessToken(User))
				{
					return Unauthorized();
				}

				if (request.UserId == 0) BadRequest();


				if (request.ProductIds.Count == 0)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "product id invalid!", false, new()));
				}

				var user = userRepository.GetUserById(request.UserId);

				if (user == null)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "User not found!", false, new()));
				}

				var resultCheckProductExist = productRepository.CheckProductExist(request.ProductIds);

				if (!resultCheckProductExist)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Product not found!", false, new()));
				}

				bool resultCheckIsExistWishList = wishListRepository.IsExistWishList(request.ProductIds, request.UserId);

				if (!resultCheckIsExistWishList)
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Wish list does not exist!", false, new()));
				}

				wishListRepository.RemoveWishListSelecteds(request.ProductIds, request.UserId);
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success!", false, new()));
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
		#endregion

	}
}
