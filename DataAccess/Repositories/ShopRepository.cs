using BusinessObject.Entities;
using DataAccess.DAOs;
using DataAccess.IRepositories;
using DTOs.Shop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
	public class ShopRepository : IShopRepository
	{
		public bool IsExistShopName(string shopName) => ShopDAO.Instance.IsExistShopName(shopName);

		public void AddShop(string avatarUrl,string shopName, long userId, string shopDescription) => ShopDAO.Instance.AddShop(avatarUrl, shopName, userId, shopDescription);

		public Shop? GetShopById(long shopId) => ShopDAO.Instance.GetShopById(shopId);

		public bool IsExistShop(long userId) =>  ShopDAO.Instance.IsExistShop(userId);

		public void EditShop(Shop shop) => ShopDAO.Instance.EditShop(shop);
	}
}
