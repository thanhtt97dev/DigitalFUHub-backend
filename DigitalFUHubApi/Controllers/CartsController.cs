using AutoMapper;
using BusinessObject.Entities;
using DataAccess.IRepositories;
using DigitalFUHubApi.Comons;
using DigitalFUHubApi.Hubs;
using DigitalFUHubApi.Managers;
using DTOs.Cart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Comons;
using DataAccess.DAOs;
using DataAccess.Repositories;

namespace DigitalFUHubApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartsController : ControllerBase
    {

        private readonly ICartRepository cartRepository;
		private readonly IProductRepository productRepository;
		private readonly IUserRepository userRepository;
		private readonly IShopRepository shopRepository;
		private readonly IAssetInformationRepository assetInformationRepository;
		private readonly IMapper mapper;

		public CartsController(ICartRepository cartRepository, IProductRepository productRepository, IUserRepository userRepository, IShopRepository shopRepository, IAssetInformationRepository assetInformationRepository, IMapper mapper)
		{
			this.cartRepository = cartRepository;
			this.productRepository = productRepository;
			this.userRepository = userRepository;
			this.shopRepository = shopRepository;
			this.assetInformationRepository = assetInformationRepository;
			this.mapper = mapper;
		}

		[HttpPost("addProductToCart")]
        //[Authorize]
        public IActionResult AddProductToCart([FromBody] AddProductToCartRequestDTO request)
        {
            try
            {
				ResponseData responseData = new ResponseData();
				ResponseData responseDataNotFound = new ResponseData
                {
                    Status = new Status 
                    { 
                        ResponseCode= Constants.RESPONSE_CODE_DATA_NOT_FOUND,
                        Ok = false,
                        Message = "Data not found!"
                    }
                };
				if (!ModelState.IsValid)
                {
                    return BadRequest();
                }

                if(request.Quantity <= 0 || request.UserId == request.ShopId) 
                {
					responseData.Status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
					responseData.Status.Ok = false;
					responseData.Status.Message = "Invalid params!";
					return Ok(responseData);
				}

				//check product, customer, shop existed
				ProductVariant? productVariant = productRepository.GetProductVariant(request.ProductVariantId);
                if (productVariant == null) return Ok(responseDataNotFound);
				User? user = userRepository.GetUserById(request.UserId);
				if (user == null) return Ok(responseDataNotFound);
				Shop? shop = shopRepository.GetShopById(request.ShopId);
				if (shop == null) return Ok(responseDataNotFound);

				// check product in shop
				if (!cartRepository.CheckProductVariantInShop(request.ShopId, request.ProductVariantId))
                {
					responseData.Status.ResponseCode = Constants.RESPONSE_CODE_CART_PRODUCT_VARIANT_NOT_IN_SHOP;
					responseData.Status.Ok = false;
					responseData.Status.Message = "Product variant  not existed in shop!";
					return Ok(responseData);
				}

				// check valid quantity to add product ti cart
				(bool isValidQuantityAddProductToCart, int numberAssetInfomation) = cartRepository.CheckValidQuantityAddProductToCart(request.UserId, request.ShopId, request.ProductVariantId, request.Quantity);

				if (!isValidQuantityAddProductToCart)
                {
					responseData.Status.ResponseCode = Constants.RESPONSE_CODE_CART_PRODUCT_INVALID_QUANTITY;
					responseData.Status.Ok = false;
					responseData.Status.Message = "Not enough quantity to add!";
                    responseData.Result = numberAssetInfomation;
					return Ok(responseData);
				}

                cartRepository.AddProductToCart(request.UserId, request.ShopId, request.ProductVariantId, request.Quantity);


				responseData.Status.ResponseCode = Constants.RESPONSE_CODE_CART_SUCCESS;
                responseData.Status.Ok = true;
				responseData.Status.Message = "Success!";
				return Ok(responseData);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(new Status());
            }
        }


        [HttpGet("GetCartsByUserId/{userId}")]
        //[Authorize]
        public IActionResult GetCartsByUserId(long userId)
        {
            try
            {
                if (userId == 0)
                {
                    return BadRequest(new Status());
                }
				List<Cart> carts = cartRepository.GetCartsByUserId(userId);

                var result = mapper.Map<List<UserCartResponseDTO>>(carts);
                return Ok(result);
            }
            catch (Exception ex)
            {
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
        }

        [HttpPut("Update")]
        [Authorize]
        public IActionResult UpdateCart(UpdateCartRequestDTO request)
        {
            try
            {
				ResponseData responseData = new ResponseData();
				if(!ModelState.IsValid) 
                {
                    return BadRequest();
                }

                if(request.Quantity <= 0)
                {
					responseData.Status.ResponseCode = Constants.RESPONSE_CODE_CART_INVALID_QUANTITY;
					responseData.Status.Ok = false;
					responseData.Status.Message = "Invalid quantity!";
					return Ok(responseData);
				}

				var cartDetail = cartRepository.GetCartDetail(request.CartDetailId);
                if (cartDetail == null)
                {
					responseData.Status.ResponseCode = Constants.RESPONSE_CODE_DATA_NOT_FOUND;
					responseData.Status.Ok = false;
					responseData.Status.Message = "Cart detail not found!";
					return Ok(responseData);
				}

                if (cartDetail.ProductVariantId != request.ProductVariantId) 
                {
					responseData.Status.ResponseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
					responseData.Status.Ok = false;
					responseData.Status.Message = "Invalid cart detail with product variant!";
					return Ok(responseData);
				}

				int numberQuantityProductVariantAvailable = assetInformationRepository
                    .GetQuantityAssetInformationProductVariantAvailable(request.ProductVariantId);
                if(numberQuantityProductVariantAvailable < request.Quantity)
                {
					responseData.Status.ResponseCode = Constants.RESPONSE_CODE_CART_PRODUCT_INVALID_QUANTITY;
					responseData.Status.Ok = false;
					responseData.Status.Message = "Not enough quantity to update!";
					responseData.Result = numberQuantityProductVariantAvailable;
					return Ok(responseData);
				}

                // update quantity in cart
                cartRepository.UpdateQuantityCartDetail(request.CartDetailId, request.Quantity);

				responseData.Status.ResponseCode = Constants.RESPONSE_CODE_CART_SUCCESS;
				responseData.Status.Ok = true;
				responseData.Status.Message = "Success!";
				return Ok(responseData);
			}
            catch (Exception ex)
            {
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
        }

        [HttpPost("DeleteCartDetail")]
        [Authorize]
        public IActionResult DeleteCartDetail([FromBody] int cartDetailId)
        {
            try
            {
				ResponseData responseData = new ResponseData();
				if (!ModelState.IsValid) 
				{
					return BadRequest();
				}

				var cartDetail = cartRepository.GetCartDetail(cartDetailId);
				if(cartDetail == null) 
				{
					responseData.Status.ResponseCode = Constants.RESPONSE_CODE_DATA_NOT_FOUND;
					responseData.Status.Ok = false;
					responseData.Status.Message = "Cart detail not found!";
				}
				cartRepository.RemoveCartDetail(cartDetailId);
				
				responseData.Status.ResponseCode = Constants.RESPONSE_CODE_CART_SUCCESS;
				responseData.Status.Ok = true;
				responseData.Status.Message = "Success!";
				return Ok(responseData);
			}
            catch (Exception ex)
            {
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
        }

		[HttpPost("DeleteCart")]
		[Authorize]
		public IActionResult DeleteCart([FromBody] List<DeleteCartRequestDTO> requestDTO)
		{
			try
			{
				ResponseData responseData = new ResponseData();
				if (!ModelState.IsValid)
				{
					return BadRequest();
				}

				cartRepository.RemoveCart(requestDTO);

				responseData.Status.ResponseCode = Constants.RESPONSE_CODE_CART_SUCCESS;
				responseData.Status.Ok = true;
				responseData.Status.Message = "Success!";
				return Ok(responseData);
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
	}
}
