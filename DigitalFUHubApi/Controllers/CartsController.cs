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

        private readonly IConnectionManager _connectionManager;
        private readonly ICartRepository _cartRepository;
        private readonly IMapper _mapper;
        private readonly IAssetInformationRepository _assetInformationRepository;

        public CartsController(IConnectionManager connectionManager,
            ICartRepository cartRepository, IMapper mapper, IAssetInformationRepository assetInformationRepository)
        {
            _connectionManager = connectionManager;
            _cartRepository = cartRepository;
            _mapper = mapper;
            _assetInformationRepository = assetInformationRepository;
        }


        [HttpPost("addProductToCart")]
        [Authorize]
        public IActionResult AddProductToCart([FromBody] CartDTO addProductToCartRequest)
        {
            try
            {
                if (addProductToCartRequest == null || addProductToCartRequest.UserId == 0 ||
                    addProductToCartRequest.ProductVariantId == 0 || addProductToCartRequest.Quantity == 0)
                {
                    return BadRequest(new Status());
                }

                var cart = _cartRepository.GetCart(addProductToCartRequest.UserId, addProductToCartRequest.ProductVariantId);
                if (cart != null)
                {
                    long quantityPurchased = addProductToCartRequest.Quantity + cart.Quantity;
                    long quantityProductVariant = _assetInformationRepository.GetByProductVariantId(cart.ProductVariantId).Count();
                    if (quantityPurchased > quantityProductVariant)
                    {
                        return Ok(new Status
                        {
                            ResponseCode = Constants.CART_RESPONSE_CODE_INVALID_QUANTITY,
                            Message = $"Sản phẩm này đang có số lượng {cart.Quantity} trong giỏ hàng của bạn," +
                            $"Không thể thêm số lượng đã chọn vào giỏ hàng vì đã vượt quá số lượng sản phẩm có sẵn",
                            Ok = false
                        });
                    }
                }
                _cartRepository.AddProductToCart(addProductToCartRequest);

                return Ok(new Status
                {
                    ResponseCode = Constants.CART_RESPONSE_CODE_SUCCESS,
                    Message = "",
                    Ok = true
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("GetCartsByUserId/{userId}")]
        //[Authorize]
        public async Task<IActionResult> GetCartsByUserId(long userId)
        {
            try
            {
                return Ok(await _cartRepository.GetCartsByUserId(userId));
            }
            catch (Exception ex)
            {
                return BadRequest(new { ex.Message });
            }
        }

        [HttpPost("DeleteCart")]
        [Authorize]
        public async Task<IActionResult> DeleteCart([FromBody] DeleteCartRequestDTO deleteCartRequest)
        {
            try
            {
                await _cartRepository.DeleteCart(deleteCartRequest.UserId, deleteCartRequest.ProductVariantId);
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
