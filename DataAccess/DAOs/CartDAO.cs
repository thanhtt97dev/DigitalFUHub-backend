using BusinessObject.Entities;
using BusinessObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs.Cart;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.DAOs
{
    public class CartDAO
    {

        private static CartDAO? instance;
        private static readonly object instanceLock = new object();

        public static CartDAO Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new CartDAO();
                    }
                }
                return instance;
            }
        }

        internal void AddProductToCart(AddProductToCartRequestDTO addProductToCartRequest)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                /*
                        var cart = context.Cart
                            .FirstOrDefault(c => c.UserId == addProductToCartRequest.UserId
                            && c.ProductVariantId == addProductToCartRequest.ProductVariantId);
                        if (cart == null)
                        {
                            Cart newCart = new Cart
                            {
                                UserId = addProductToCartRequest.UserId,
                                ProductVariantId = addProductToCartRequest.ProductVariantId,
                                Quantity = addProductToCartRequest.Quantity,
                            };
                            context.Cart.Add(newCart);
                            context.SaveChanges();

                        } else
                        {
                            cart.Quantity += addProductToCartRequest.Quantity;
                            context.Cart.Update(cart);
                            context.SaveChanges();
                        }
                 */
            }

        }


        internal Cart? GetCart(long userId, long productVariantId)
        {
            return null;
            using (DatabaseContext context = new DatabaseContext())
            {
                /*
                var cart = context.Cart.FirstOrDefault(
                        c => c.UserId == userId && c.ProductVariantId == productVariantId
                    );

                return cart;
                */
            }
        }

        internal List<CartGroupResponseDTO> GetCartsByUserId(long userId)
        {
            return new List<CartDTO>();
            using (DatabaseContext context = new DatabaseContext())
            {
                /*
                List<CartDTO> cartDTOs = new List<CartDTO>();
                var carts = context.Cart.Include(_ => _.User).Where(c => c.UserId == userId).ToList();
                foreach (var cart in carts)
                {
                    var productVariant = context.ProductVariant.FirstOrDefault(p => p.ProductVariantId == cart.ProductVariantId);
                    if (productVariant == null) throw new ArgumentNullException("Not found product variant");
                    var product = context.Product.Include(_ => _.Shop).FirstOrDefault(p => p.ProductId == productVariant.ProductId);
                    //long price = productVariant.Price * cart.Quantity;
                    CartDTO cartDTO = new CartDTO()
                    {
                  
                        UserId = cart.UserId,
                        ShopName = product?.Shop.ShopName ?? "",
                        ShopId = product?.Shop.UserId ?? 0,
                        Coin = cart.User.Coin,
                        Quantity = cart.Quantity,
                        ProductVariantId = cart.ProductVariantId,
                        Product = new ProductCartResponseDTO
                        {
                            ProductId = product?.ProductId ?? 0,
                            Thumbnail = product?.Thumbnail ?? "",
                            ProductName = product?.ProductName ?? "",
                            Discount = product?.Discount ?? 0,
                        },
                        ProductVariant = new ProductVariantCartResponseDTO
                        {
                            ProductVariantName = productVariant.Name,
                            Price = productVariant.Price,
                            PriceDiscount = productVariant.Price - (productVariant.Price * (product?.Discount ?? 1) / 100),
                            Quantity = context.AssetInformation.Count(x => x.ProductVariantId == cart.ProductVariantId)
                        }
                    };
                    cartDTOs.Add(cartDTO);
                }
<<<<<<< HEAD
              
                return cartDTOs.OrderBy(c => c.ShopName).ToList();
                */
=======

                var groupCart = cartDTOs
                    .GroupBy(x => new { x.UserId, x.ShopId, x.ShopName, x.Coin })
                    .Select(group => new CartGroupResponseDTO
                    {
                        UserId = group.Key.UserId,
                        ShopId = group.Key.ShopId,
                        ShopName = group.Key.ShopName,
                        Coin = group.Key.Coin,
                        Products = group.Select(x => new ProductCartGroupResponseDTO
                        {
                            Quantity = x.Quantity,
                            ProductVariantId = x.ProductVariantId,
                            ProductVariantName = x.ProductVariant?.ProductVariantName ?? "",
                            Price = x.ProductVariant?.Price ?? 0,
                            PriceDiscount = x.ProductVariant?.PriceDiscount ?? 0,
                            ProductVariantQuantity = x.ProductVariant?.Quantity ?? 0,
                            ProductId = x.Product?.ProductId ?? 0,
                            Thumbnail = x.Product?.Thumbnail ?? "",
                            ProductName = x.Product?.ProductName ?? "",
                            Discount = x.Product?.Discount ?? 0
             
                        }).ToList()
                    }).ToList();

                return groupCart.OrderBy(c => c.ShopName).ToList();
>>>>>>> 8413a5f9ad8791ecc6441a59f62db376271b0da6
            }
        }


        internal (bool, long) CheckQuantityForCart(long userId, long productVariantId, long quantity)
        {
            return (true, 0);
            using (DatabaseContext context = new DatabaseContext())
            {
                /*
                var cart = GetCart(userId, productVariantId);
                if (cart != null)
                {
                    long quantityPurchased = quantity + cart.Quantity;
                    long quantityProductVariant = context.AssetInformation.Count(a => a.ProductVariantId == productVariantId);
                    if (quantityPurchased > quantityProductVariant)
                    {
                        return (false, cart.Quantity);
                    }
                }
                return (true, cart?.Quantity ?? 0);
                */
            }
        }

        internal void UpdateCart(Cart newCart)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                /*
                var cart = context.Cart.FirstOrDefault(x => x.UserId == newCart.UserId && x.ProductVariantId == newCart.ProductVariantId);
                if (cart == null) throw new Exception("Cart's not existed!");
                cart.ProductVariant = newCart.ProductVariant;
                cart.Quantity = newCart.Quantity != 0 ? newCart.Quantity : cart.Quantity;
                context.SaveChanges();
                */
            }
        }


        internal async Task DeleteCart(long userId, long productVariantId)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                /*
                var cart = await context.Cart.FirstOrDefaultAsync(
                        c => c.UserId == userId && c.ProductVariantId == productVariantId
                    );
                if (cart == null) throw new ArgumentNullException("Not found cart");
                context.Cart.Remove(cart);
                context.SaveChanges();
                */
            }
        }

        internal bool CheckValidQuantityAddProductToCart(long userId, long shopId, long productVariantId, int quantity)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                // check productVariant in shop
                bool isProductVariantInShop = context.ProductVariant
                    .Include(x => x.Product)
                    .Any(x => x.ProductVariantId == productVariantId && x.Product.ShopId == shopId);

                if (!isProductVariantInShop)
                {

                }
            }
            return false;
        }

		internal bool CheckProductVariantInShop(long shopId, long productVariantId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				// check productVariant in shop
				bool isProductVariantInShop = context.ProductVariant
					.Include(x => x.Product)
					.Any(x => x.ProductVariantId == productVariantId && x.Product.ShopId == shopId);

				return isProductVariantInShop;
			}
		}
	}
}

