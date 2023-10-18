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

        public void AddProductToCart(long userId, long shopId, long productVariantId, int quantity) => CartDAO.Instance.AddProductToCart(userId, shopId, productVariantId, quantity);

        public List<Cart> GetCartsByUserId(long userId) => CartDAO.Instance.GetCartsByUserId(userId);

		public bool CheckProductVariantInShop(long shopId, long productVariantId) => CartDAO.Instance.CheckProductVariantInShop(shopId, productVariantId);

		public (bool, int) CheckValidQuantityAddProductToCart(long userId, long shopId, long productVariantId, int quantity) => CartDAO.Instance.CheckValidQuantityAddProductToCart(userId,shopId ,productVariantId, quantity);

		public CartDetail? GetCartDetail(long cartDetailId) => CartDAO.Instance.GetCartDetail(cartDetailId);

		public void UpdateQuantityCartDetail(long cartDetailId, int quantity) => CartDAO.Instance.UpdateQuantityCartDetail(cartDetailId, quantity);

		public void RemoveCartDetail(long cartDetailId) => CartDAO.Instance.RemoveCartDetail(cartDetailId);

	}
}
