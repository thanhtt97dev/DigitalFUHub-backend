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

        [HttpPost("Add")]
        [Authorize]
        public IActionResult AddWishList(WishListCustomerAddRequestDTO request)
        {
            ResponseData responseData = new ResponseData();
            Status status = new Status();
            try
            {
                if (request.UserId != jwtTokenService.GetUserIdByAccessToken(User))
                {
                    return Unauthorized();
                }

                (string responseCode, string message, bool isOk) = wishListRepository.CheckRequestWishListIsValid(request.ProductId, request.UserId);
                if (!isOk)
                {
                    status.ResponseCode = responseCode;
                    status.Message = message;
                    status.Ok = isOk;
                    responseData.Status = status;
                    return Ok(responseData);
                }

                var product = productRepository.GetProductEntityById(request.ProductId);

                if (product == null)
                {
                    status.ResponseCode = Constants.RESPONSE_CODE_DATA_NOT_FOUND;
                    status.Message = "Product not found";
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

                bool isWishList = wishListRepository.IsExistWishList(request.ProductId, request.UserId);

                if (isWishList)
                {
                    status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
                    status.Message = "Wish list already exists";
                    status.Ok = false;
                    responseData.Status = status;
                    return Ok(responseData);
                }

                wishListRepository.AddWishList(request.ProductId, request.UserId);
                status.ResponseCode = Constants.RESPONSE_CODE_SUCCESS;
                status.Message = "Success";
                status.Ok = true;
                responseData.Status = status;
                return Ok(responseData);

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }


        [HttpPost("Remove")]
        [Authorize]
        public IActionResult RemoveWishList(WishListCustomerRemoveRequestDTO request)
        {
            ResponseData responseData = new ResponseData();
            Status status = new Status();
            try
            {
                if (request.UserId != jwtTokenService.GetUserIdByAccessToken(User))
                {
                    return Unauthorized();
                }

                (string responseCode, string message, bool isOk) = wishListRepository.CheckRequestWishListIsValid(request.ProductId, request.UserId);
                if (!isOk)
                {
                    status.ResponseCode = responseCode;
                    status.Message = message;
                    status.Ok = isOk;
                    responseData.Status = status;
                    return Ok(responseData);
                }

                var product = productRepository.GetProductEntityById(request.ProductId);

                if (product == null)
                {
                    status.ResponseCode = Constants.RESPONSE_CODE_DATA_NOT_FOUND;
                    status.Message = "Product not found";
                    status.Ok = false;
                    responseData.Status = status;
                    return Ok(responseData);
                }

                bool isWishList = wishListRepository.IsExistWishList(request.ProductId, request.UserId);

                if (!isWishList)
                {
                    status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
                    status.Message = "Wish list does not exist";
                    status.Ok = false;
                    responseData.Status = status;
                    return Ok(responseData);
                }

                wishListRepository.RemoveWishList(request.ProductId, request.UserId);
                status.ResponseCode = Constants.RESPONSE_CODE_SUCCESS;
                status.Message = "Success";
                status.Ok = true;
                responseData.Status = status;
                return Ok(responseData);

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("IsWishList")]
        [Authorize]
        public IActionResult CheckIsWistList (long productId, long userId)
        {
            ResponseData responseData = new ResponseData();
            Status status = new Status();
            try
            {
                if (userId != jwtTokenService.GetUserIdByAccessToken(User))
                {
                    return Unauthorized();
                }

                (string responseCode, string message, bool isOk) = wishListRepository.CheckRequestWishListIsValid(productId, userId);
                if (!isOk)
                {
                    status.ResponseCode = responseCode;
                    status.Message = message;
                    status.Ok = isOk;
                    responseData.Status = status;
                    return Ok(responseData);
                }

                var product = productRepository.GetProductEntityById(productId);

                if (product == null)
                {
                    status.ResponseCode = Constants.RESPONSE_CODE_DATA_NOT_FOUND;
                    status.Message = "Product not found";
                    status.Ok = false;
                    responseData.Status = status;
                    return Ok(responseData);
                }

                bool isWishList = wishListRepository.IsExistWishList(productId, userId);
                status.ResponseCode = Constants.RESPONSE_CODE_SUCCESS;
                status.Message = "Success";
                status.Ok = true;
                responseData.Status = status;
                responseData.Result = isWishList;
                return Ok(responseData);

            } catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("GetAll")]
        //[Authorize]
        public IActionResult GetWishListByUserId(WishListCustomerParamRequestDTO request)
        {
            ResponseData responseData = new ResponseData();
            Status status = new Status();
            try
            {
                //if (request.UserId != jwtTokenService.GetUserIdByAccessToken(User))
                //{
                //    return Unauthorized();
                //}

                var user = userRepository.GetUserById(request.UserId);
                if (user == null)
                {
                    status.ResponseCode = Constants.RESPONSE_CODE_DATA_NOT_FOUND;
                    status.Message = "User not found";
                    status.Ok = false;
                    responseData.Status = status;
                    return Ok(responseData);

                }

                if (request.Page <= 0)
                {
                    status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
                    status.Message = "Invalid param";
                    status.Ok = false;
                    responseData.Status = status;
                    return Ok(responseData);
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

                if (request.UserId == 0)
                {
                    status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
                    status.Message = "user id invalid!";
                    status.Ok = false;
                    responseData.Status = status;
                    return Ok(responseData);
                }

                if (request.ProductIds.Count == 0)
                {
                    status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
                    status.Message = "product id invalid!";
                    status.Ok = false;
                    responseData.Status = status;
                    return Ok(responseData);
                }

                var user = userRepository.GetUserById(request.UserId);

                if (user == null)
                {
                    status.ResponseCode = Constants.RESPONSE_CODE_DATA_NOT_FOUND;
                    status.Message = "User not found";
                    status.Ok = false;
                    responseData.Status = status;
                    return Ok(responseData);
                }

                var resultCheckProductExist = productRepository.CheckProductExist(request.ProductIds);

                if (!resultCheckProductExist)
                {
                    status.ResponseCode = Constants.RESPONSE_CODE_DATA_NOT_FOUND;
                    status.Message = "Product not found";
                    status.Ok = false;
                    responseData.Status = status;
                    return Ok(responseData);
                }

                bool resultCheckIsExistWishList = wishListRepository.IsExistWishList(request.ProductIds, request.UserId);

                if (!resultCheckIsExistWishList)
                {
                    status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
                    status.Message = "Wish list does not exist";
                    status.Ok = false;
                    responseData.Status = status;
                    return Ok(responseData);
                }

                wishListRepository.RemoveWishListSelecteds(request.ProductIds, request.UserId);
                status.ResponseCode = Constants.RESPONSE_CODE_SUCCESS;
                status.Message = "Success";
                status.Ok = true;
                responseData.Status = status;
                return Ok(responseData);

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
