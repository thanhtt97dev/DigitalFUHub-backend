using BusinessObject.Entities;
using DTOs.Shop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.IRepositories
{
	public interface IShopRepository
	{
		Task CreateShopAsync(RegisterShopRequestDTO request);
		Task<Product> GetProductByIdAsync(long productId);
		Task<bool> ShopHasProductAsync(long userId, long productId);
		Task<bool> UserHasShopAsync(long userId);
	}
}
