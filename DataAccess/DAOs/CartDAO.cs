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

        internal void AddProductToCart(CartDTO addProductToCartRequest)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
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
                    }
        }


        internal Cart? GetCart(long userId, long productVariantId)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                var cart = context.Cart.FirstOrDefault(
                        c => c.UserId == userId && c.ProductVariantId == productVariantId
                    );

                return cart;
            }
        }

        internal async Task<List<CartDTO>> GetCartsByUserId(long userId)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                List<CartDTO> cartDTOs = new List<CartDTO>();
                var carts = await context.Cart.Where(c => c.UserId == userId).ToListAsync();
                foreach (var cart in carts)
                {
                    var productVariant = await context.ProductVariant.FirstOrDefaultAsync(p => p.ProductVariantId == cart.ProductVariantId);
                    if (productVariant == null) throw new ArgumentNullException("Not found product variant");
                    var product = await context.Product.Include(_ => _.Shop).FirstOrDefaultAsync(p => p.ProductId == productVariant.ProductId);
                    long price = productVariant.Price * cart.Quantity;
                    CartDTO cartDTO = new CartDTO()
                    {
                        ProductVariantId = cart.ProductVariantId,
                        UserId = cart.UserId,
                        Quantity = cart.Quantity,
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
                            Price = price,
                            PriceDiscount = price - (price * (product?.Discount ?? 1) / 100),
                            Quantity = context.AssetInformation.Count(x => x.ProductVariantId == cart.ProductVariantId)
                        },
                        ShopName = product?.Shop.ShopName ?? ""
                    };
                    cartDTOs.Add(cartDTO);
                }
              
                return cartDTOs.OrderBy(c => c.ShopName).ToList();
            }
        }


        internal async Task DeleteCart(long userId, long productVariantId)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                var cart = await context.Cart.FirstOrDefaultAsync(
                        c => c.UserId == userId && c.ProductVariantId == productVariantId
                    );
                if (cart == null) throw new ArgumentNullException("Not found cart");
                context.Cart.Remove(cart);
                context.SaveChanges();
            }
        }


    }
}

