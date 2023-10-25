using BusinessObject.Entities;
using DTOs.Cart;
using DTOs.Seller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.IRepositories
{
    public interface ICartRepository
    {
		void AddProductToCart(long userId, long shopId, long productVariantId, int quantity);
        List<Cart> GetCartsByUserId(long userId);
        CartDetail? GetCartDetail(long CartDetailId);
        void UpdateQuantityCartDetail(long cartDetailId, int quantity);
		void RemoveCartDetail(long cartDetailId);
        void RemoveCart(List<DeleteCartRequestDTO> deleteCartRequestDTO);
		(bool, int) CheckValidQuantityAddProductToCart(long userId,long shopId ,long productVariantId, int quantity);
        bool CheckProductVariantInShop(long shopId, long productVariantId);
    }
}
