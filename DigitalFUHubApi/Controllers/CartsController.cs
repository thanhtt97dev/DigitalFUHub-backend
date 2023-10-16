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
					responseData.Status.ResponseCode = Constants.CART_RESPONSE_CODE_CART_PRODUCT_VARIANT_NOT_IN_SHOP;
					responseData.Status.Ok = false;
					responseData.Status.Message = "Product variant  not existed in shop!";
					return Ok(responseData);
				}

				// check valid quantity to add product ti cart
				(bool isValidQuantityAddProductToCart, int numberAssetInfomation) = cartRepository.CheckValidQuantityAddProductToCart(request.UserId, request.ShopId, request.ProductVariantId, request.Quantity);

				if (!isValidQuantityAddProductToCart)
                {
					responseData.Status.ResponseCode = Constants.CART_RESPONSE_CODE_INVALID_QUANTITY;
					responseData.Status.Ok = false;
					responseData.Status.Message = "Not enough quantity to add!";
                    responseData.Result = numberAssetInfomation;
					return Ok(responseData);
				}

                cartRepository.AddProductToCart(request.UserId, request.ShopId, request.ProductVariantId, request.Quantity);

                return Ok(new Status
                {
                    ResponseCode = Constants.CART_RESPONSE_CODE_SUCCESS,
                    Message = "",
                    Ok = true
                });
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
                return BadRequest(new { ex.Message });
            }
        }

        [HttpPut("Update")]
        [Authorize]
        public IActionResult UpdateCart(UpdateCartRequestDTO? updateCartRequest)
        {
            try
            {
                if (updateCartRequest == null || updateCartRequest.UserId == 0 ||
                        updateCartRequest.ProductVariantId == 0)
                {
                    return BadRequest(new Status());
                }
                long quantityProductVariant = assetInformationRepository.GetByProductVariantId(updateCartRequest.ProductVariantId).Count();
                if (updateCartRequest.Quantity == 0)
                {
                    var cart = cartRepository.GetCart(updateCartRequest.UserId, updateCartRequest.ProductVariantId);
                    if (cart != null)
                    {
                        if(true)
                        //if (cart.Quantity > quantityProductVariant)
                        {
                            
                            updateCartRequest.Quantity = quantityProductVariant;
                            cartRepository.UpdateCart(mapper.Map<Cart>(updateCartRequest));
                            return Ok(new Status
                            {
                                ResponseCode = Constants.CART_RESPONSE_CODE_CART_PRODUCT_INVALID_QUANTITY,
                                Message = $"Rất tiếc, bạn chỉ có thể mua số lượng tối đa {quantityProductVariant} sản phẩm " +
                                $"(Số lượng sản phẩm trong giỏ hàng của bạn đã được thay đổi)",
                                Ok = false
                            });
                        } else {
                            return Ok(new Status
                            {
                                ResponseCode = Constants.CART_RESPONSE_CODE_SUCCESS,
                                Message = "",
                                Ok = true
                            });
                        }
                    }
                    else
                    {
                        return NotFound(new Status());
                    }
                }

                var resultCheck = !(updateCartRequest.Quantity > quantityProductVariant);
                if (!resultCheck)
                {
                    updateCartRequest.Quantity = quantityProductVariant;
                    cartRepository.UpdateCart(mapper.Map<Cart>(updateCartRequest));
                    return Ok(new Status
                    {
                        ResponseCode = Constants.CART_RESPONSE_CODE_INVALID_QUANTITY,
                        Message = $"Sản phẩm này đang có số lượng tối đa là {quantityProductVariant} " +
                        $"(số lượng sản phẩm trong giỏ hàng của bạn đã được thay đổi thành {quantityProductVariant})",
                        Ok = resultCheck
                    });
                }

                cartRepository.UpdateCart(mapper.Map<Cart>(updateCartRequest));
                return Ok(new Status
                {
                    ResponseCode = Constants.CART_RESPONSE_CODE_SUCCESS,
                    Message = "",
                    Ok = true
                });


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(new Status());
            }
        }

        [HttpPost("DeleteCart")]
        [Authorize]
        public async Task<IActionResult> DeleteCart([FromBody] DeleteCartRequestDTO deleteCartRequest)
        {
            try
            {
                await cartRepository.DeleteCart(deleteCartRequest.UserId, deleteCartRequest.ProductVariantId);
                return Ok(new Status
                {
                    Message = "Delete Cart Successfully",
                    ResponseCode = Constants.RESPONSE_CODE_SUCCESS,
                    Ok = true
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new Status
                {
                    Message = ex.Message
                    ,
                    ResponseCode = Constants.RESPONSE_CODE_FAILD
                    ,
                    Ok = false
                });
            }
        }
    }
}
