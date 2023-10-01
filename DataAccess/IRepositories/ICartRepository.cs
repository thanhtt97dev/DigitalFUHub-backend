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
        Task AddProductToCart(CartDTO addProductRequest);

        Task<Cart?> GetCart(long userId, long productVariantId);

        Task<List<CartDTO>> GetCartsByUserId(long userId);
    }
}
