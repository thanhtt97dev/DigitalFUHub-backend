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

namespace DigitalFUHubApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartsController : ControllerBase
    {

        private readonly IConnectionManager _connectionManager;
        private readonly ICartRepository _cartRepository;
        private readonly IMapper _mapper;

        public CartsController(IConnectionManager connectionManager,
            ICartRepository cartRepository, IMapper mapper)
        {
            _connectionManager = connectionManager;
            _cartRepository = cartRepository;
            _mapper = mapper;
        }


        [HttpPost("addProductToCart")]
        [Authorize]
        public async Task<IActionResult> AddProductToCart([FromBody] CartDTO addProductToCartRequest)
        {
            try
            {
                await _cartRepository.AddProductToCart(addProductToCartRequest);
                return Ok(new Status
                {
                    Message = "Add Product To Cart Successfully"
                    ,
                    ResponseCode = Constants.RESPONSE_CODE_SUCCESS
                    ,
                    Ok = true
                });

            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("GetCart")]
        [Authorize]
        public async Task<IActionResult> GetCart (long userId, long productVariantId)
        {
            try
            {
                return Ok(_mapper.Map<CartDTO>(await _cartRepository.GetCart(userId, productVariantId)));
            } catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("GetCartsByUserId/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetCartsByUserId(long userId)
        {
            try
            {
                return Ok(await _cartRepository.GetCartsByUserId(userId));
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpDelete("DeleteCart")]
        [Authorize]
        public async Task<IActionResult> DeleteCart([FromBody] DeleteCartRequestDTO deleteCartRequest)
        {
            try
            {
                await _cartRepository.DeleteCart(deleteCartRequest.UserId, deleteCartRequest.ProductVariantId);
                return Ok(new Status
                {
                    Message = "Delete Cart Successfully"
                    ,
                    ResponseCode = Constants.RESPONSE_CODE_SUCCESS
                    ,
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
