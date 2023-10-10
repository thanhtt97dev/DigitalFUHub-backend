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
		bool CheckShopNameExisted(string v);
		void CreateShop(RegisterShopRequestDTO request);
		Product GetProductById(long productId);
		bool ShopHasProduct(long userId, long productId);
		bool UserHasShop(long userId);

    }
}
