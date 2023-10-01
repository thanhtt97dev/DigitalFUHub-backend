using BusinessObject.Entities;
using DataAccess.DAOs;
using DataAccess.IRepositories;
using DTOs.Cart;
using DTOs.Seller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class CartRepository : ICartRepository
    {

        public async Task AddProductToCart(CartDTO addProductToCartRequest)
        {
            if (addProductToCartRequest == null)
            {
                throw new ArgumentNullException(nameof(addProductToCartRequest), "The addProductToCartRequest request object must not be null");
            }

            await CartDAO.Instance.AddProductToCart(addProductToCartRequest);  
                }

        public async Task<Cart?> GetCart(long userId, long productVariantId)
        {
            if (userId == 0)
            {
                throw new ArgumentException(nameof(userId), "UserId must be difference from 0");
            }
            if (productVariantId == 0)
            {
                throw new ArgumentException(nameof(userId), "ProductVariantId must be difference from 0");
            }

            var cart = await CartDAO.Instance.GetCart(userId, productVariantId); 

            return cart;

        }
    }
}
