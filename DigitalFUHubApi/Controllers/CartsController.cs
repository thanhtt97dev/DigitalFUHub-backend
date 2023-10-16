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

        private readonly IConnectionManager connectionManager;
        private readonly ICartRepository cartRepository;
        private readonly IMapper mapper;
        private readonly IAssetInformationRepository assetInformationRepository;

		public CartsController(IConnectionManager connectionManager, ICartRepository cartRepository, IMapper mapper, IAssetInformationRepository assetInformationRepository)
		{
			this.connectionManager = connectionManager;
			this.cartRepository = cartRepository;
			this.mapper = mapper;
			this.assetInformationRepository = assetInformationRepository;
		}

		[HttpPost("addProductToCart")]
        //[Authorize]
        public IActionResult AddProductToCart([FromBody] AddProductToCartRequestDTO request)
        {
            try
            {
				ResponseData responseData = new ResponseData();
				if (!ModelState.IsValid)
                {
                    return BadRequest();
                }

                // check product in shop
                if(!cartRepository.CheckProductVariantInShop(request.ShopId, request.ProductVariantId))
                {
					responseData.Status.ResponseCode = Constants.CART_RESPONSE_CODE_CART_PRODUCT_VARIANT_NOT_IN_SHOP;
					responseData.Status.Ok = false;
					responseData.Status.Message = "Product variant  not existed in shop!";
					return Ok(responseData);
				}

                // check valid quantity to add product ti cart
                if(cartRepository.CheckValidQuantityAddProductToCart(request.UserId, request.ShopId, request.ProductVariantId, request.Quantity))
                {
					responseData.Status.ResponseCode = Constants.CART_RESPONSE_CODE_INVALID_QUANTITY;
					responseData.Status.Ok = false;
					responseData.Status.Message = (request.Quantity - 1).ToString();
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
