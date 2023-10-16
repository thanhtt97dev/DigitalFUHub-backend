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

        public void AddProductToCart(CartDTO addProductToCartRequest) => CartDAO.Instance.AddProductToCart(addProductToCartRequest);

        public (bool, long) CheckQuantityForCart(long userId, long productVariantId, long quantity) => CartDAO.Instance.CheckQuantityForCart(userId, productVariantId, quantity);

        public async Task DeleteCart(long userId, long productVariantId)
        {
            if (userId == 0)
            {
                throw new ArgumentException(nameof(userId), "UserId must be difference from 0");
            }
            if (productVariantId == 0)
            {
                throw new ArgumentException(nameof(userId), "ProductVariantId must be difference from 0");
            }

            await CartDAO.Instance.DeleteCart(userId, productVariantId);
        }

        public Cart? GetCart(long userId, long productVariantId) => CartDAO.Instance.GetCart(userId, productVariantId);

        public async Task<List<CartDTO>> GetCartsByUserId(long userId)
        {
            if (userId == 0)
            {
                throw new ArgumentException(nameof(userId), "UserId must be difference from 0");
            }

            return await CartDAO.Instance.GetCartsByUserId(userId);
        }

		public bool CheckProductVariantInShop(long shopId, long productVariantId) => CartDAO.Instance.CheckProductVariantInShop(shopId, productVariantId);

		public bool CheckValidQuantityAddProductToCart(long userId, long shopId, long productVariantId, int quantity) => CartDAO.Instance.CheckValidQuantityAddProductToCart(userId,shopId ,productVariantId, quantity);

		public void UpdateCart(Cart newCart) => CartDAO.Instance.UpdateCart(newCart);
    }
}
