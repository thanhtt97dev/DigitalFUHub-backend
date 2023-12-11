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
using Azure.Core;
using DigitalFUHubApi.Services;

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
        private readonly JwtTokenService jwtTokenService;
        private readonly IMapper mapper;

		public CartsController(ICartRepository cartRepository, JwtTokenService jwtTokenService, IProductRepository productRepository, IUserRepository userRepository, IShopRepository shopRepository, IAssetInformationRepository assetInformationRepository, IMapper mapper)
		{
			this.cartRepository = cartRepository;
			this.productRepository = productRepository;
			this.userRepository = userRepository;
			this.shopRepository = shopRepository;
			this.assetInformationRepository = assetInformationRepository;
			this.jwtTokenService = jwtTokenService;
            this.mapper = mapper;
		}

        #region add product to cart
        [Authorize]
        [HttpPost("addProductToCart")]
		public IActionResult AddProductToCart([FromBody] AddProductToCartRequestDTO request)
        {
            try
            {
				if (!ModelState.IsValid) return BadRequest();

                if(request.Quantity <= 0 || request.UserId == request.ShopId)
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid params", false, new()));

				//check product, customer, shop existed
				ProductVariant? productVariant = productRepository.GetProductVariant(request.ProductVariantId);
                if (productVariant == null)
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Data not found!", false, new()));
				User? user = userRepository.GetUserById(request.UserId);
				if (user == null)
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Data not found!", false, new()));
				Shop? shop = shopRepository.GetShopById(request.ShopId);
				if (shop == null)
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Data not found!", false, new()));

				// check product in shop
				if (!cartRepository.CheckProductVariantInShop(request.ShopId, request.ProductVariantId))
                {
					return Ok(new ResponseData(Constants.RESPONSE_CODE_CART_PRODUCT_VARIANT_NOT_IN_SHOP, "Product variant  not existed in shop!", false, new()));
				}

				// check valid quantity to add product ti cart
				(bool isValidQuantityAddProductToCart, int numberAssetInfomation) = cartRepository.CheckValidQuantityAddProductToCart(request.UserId, request.ShopId, request.ProductVariantId, request.Quantity);

				if (!isValidQuantityAddProductToCart)
                {
					return Ok(new ResponseData(Constants.RESPONSE_CODE_CART_PRODUCT_INVALID_QUANTITY, "Not enough quantity to add!", false, numberAssetInfomation));
				}

                cartRepository.AddProductToCart(request.UserId, request.ShopId, request.ProductVariantId, request.Quantity);

				return Ok(new ResponseData(Constants.RESPONSE_CODE_CART_SUCCESS, "Success!", true, new()));
            }
            catch (Exception)
            {
                return BadRequest(new Status());
            }
        }
        #endregion

        #region get carts by userid
        [HttpGet("GetCartsByUserId/{userId}")]
		[Authorize]
		public IActionResult GetCartsByUserId(long userId)
        {
            try
            {
                if (userId == 0)
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "userId invalid!!", false, new()));

				if (userId != jwtTokenService.GetUserIdByAccessToken(User)) return Unauthorized();

				var user = userRepository.GetUserById(userId);
				if (user == null)
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "user not found!", false, new()));

				List<Cart> carts = cartRepository.GetCartsByUserId(userId);
                var result = mapper.Map<List<UserCartResponseDTO>>(carts);

				// Ok
				return Ok(new ResponseData(Constants.RESPONSE_CODE_SUCCESS, "Success!", true, result));
            }
            catch (Exception ex)
            {
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
        }
        #endregion

        #region update cart
        [HttpPut("Update")]
        [Authorize]
        public IActionResult UpdateCart(UpdateCartRequestDTO request)
        {
            try
            {
				if(!ModelState.IsValid) return BadRequest();

                if(request.Quantity <= 0)
                {
					return Ok(new ResponseData(Constants.RESPONSE_CODE_CART_INVALID_QUANTITY, "Invalid quantity!", false, new()));
				}

				var cartDetail = cartRepository.GetCartDetail(request.CartDetailId);
                if (cartDetail == null)
                {
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Cart detail not found!", false, new()));
				}

                if (cartDetail.ProductVariantId != request.ProductVariantId) 
                {
					return Ok(new ResponseData(Constants.RESPONSE_CODE_NOT_ACCEPT, "Invalid cart detail with product variant!", false, new()));
				}

				int numberQuantityProductVariantAvailable = assetInformationRepository
                    .GetQuantityAssetInformationProductVariantAvailable(request.ProductVariantId);
                if(numberQuantityProductVariantAvailable < request.Quantity)
                {
					return Ok(new ResponseData(Constants.RESPONSE_CODE_CART_PRODUCT_INVALID_QUANTITY, "Not enough quantity to update!", false, numberQuantityProductVariantAvailable));
				}

                // update quantity in cart
                cartRepository.UpdateQuantityCartDetail(request.CartDetailId, request.Quantity);

				return Ok(new ResponseData(Constants.RESPONSE_CODE_CART_SUCCESS, "Success!", true, new()));
			}
            catch (Exception ex)
            {
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
        }
        #endregion

        #region delete cart detail
        [HttpPost("DeleteCartDetail")]
        [Authorize]
        public IActionResult DeleteCartDetail([FromBody] int cartDetailId)
        {
            try
            {
				if (!ModelState.IsValid) return BadRequest();

				var cartDetail = cartRepository.GetCartDetail(cartDetailId);
				if(cartDetail == null) 
				{
					return Ok(new ResponseData(Constants.RESPONSE_CODE_DATA_NOT_FOUND, "Cart detail not found!", false, new()));
				}
				cartRepository.RemoveCartDetail(cartDetailId);

				return Ok(new ResponseData(Constants.RESPONSE_CODE_CART_SUCCESS, "Success!", true, new()));
			}
            catch (Exception ex)
            {
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
        }
        #endregion

        #region delete cart
        [HttpPost("DeleteCart")]
		[Authorize]
		public IActionResult DeleteCart([FromBody] List<DeleteCartRequestDTO> requestDTO)
		{
			try
			{
				if (!ModelState.IsValid) return BadRequest();

				cartRepository.RemoveCart(requestDTO);

				return Ok(new ResponseData(Constants.RESPONSE_CODE_CART_SUCCESS, "Success!", true, new()));
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
			}
		}
        #endregion
    }
}
