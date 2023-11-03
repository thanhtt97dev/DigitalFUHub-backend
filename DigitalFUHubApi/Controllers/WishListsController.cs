using AutoMapper;
using Azure.Core;
using BusinessObject.Entities;
using Comons;
using DataAccess.IRepositories;
using DigitalFUHubApi.Comons;
using DigitalFUHubApi.Services;
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
        private readonly IMapper mapper;

        public WishListsController(IUserRepository userRepository, JwtTokenService jwtTokenService, IWishListRepository wishListRepository, IMapper mapper) {
            this.jwtTokenService = jwtTokenService;
            this.userRepository = userRepository;
            this.wishListRepository = wishListRepository;
            this.mapper = mapper;
        }

        [HttpPost("Add")]
        [Authorize]
        public IActionResult AddWishList(AddWishListRequestDTO request)
        {
            ResponseData responseData = new ResponseData();
            Status status = new Status();
            try
            {
                //if (request.UserId != jwtTokenService.GetUserIdByAccessToken(User))
                //{
                //    return Unauthorized();
                //}

                (string responseCode, string message, bool isOk) = wishListRepository.CheckRequestWishListIsValid(request.ProductId, request.UserId);
                if (!isOk)
                {
                    status.ResponseCode = responseCode;
                    status.Message = message;
                    status.Ok = isOk;
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
        public IActionResult RemoveWishList(RemoveWishListRequestDTO request)
        {
            ResponseData responseData = new ResponseData();
            Status status = new Status();
            try
            {
                //if (request.UserId != jwtTokenService.GetUserIdByAccessToken(User))
                //{
                //    return Unauthorized();
                //}

                (string responseCode, string message, bool isOk) = wishListRepository.CheckRequestWishListIsValid(request.ProductId, request.UserId);
                if (!isOk)
                {
                    status.ResponseCode = responseCode;
                    status.Message = message;
                    status.Ok = isOk;
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
                //if (userId != jwtTokenService.GetUserIdByAccessToken(User))
                //{
                //    return Unauthorized();
                //}

                (string responseCode, string message, bool isOk) = wishListRepository.CheckRequestWishListIsValid(productId, userId);
                if (!isOk)
                {
                    status.ResponseCode = responseCode;
                    status.Message = message;
                    status.Ok = isOk;
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

        [HttpGet("GetByUserId/{userId}")]
        [Authorize]
        public IActionResult GetWishListByUserId(long userId)
        {
            ResponseData responseData = new ResponseData();
            Status status = new Status();
            try
            {
                //if (userId != jwtTokenService.GetUserIdByAccessToken(User))
                //{
                //    return Unauthorized();
                //}

                var user = userRepository.GetUserById(userId);
                if (user == null)
                {
                    status.ResponseCode = Constants.RESPONSE_CODE_DATA_NOT_FOUND;
                    status.Message = "User not found";
                    status.Ok = false;
                }

                var products = mapper.Map<List<WishListProductResponseDTO>>(wishListRepository.GetProductFromWishListByUserId(userId));

                status.ResponseCode = Constants.RESPONSE_CODE_SUCCESS;
                status.Message = "Success";
                status.Ok = true;
                responseData.Status = status;
                responseData.Result = products;
                return Ok(responseData);

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
