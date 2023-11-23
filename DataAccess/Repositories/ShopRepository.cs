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

		public int GetNumberShopWithCondition(long shopId, string shopEmail, string shopName, int shopStatusId) => ShopDAO.Instance.GetNumberShopWithCondition(shopId, shopEmail, shopName, shopStatusId);

		public List<Shop> GetShopsWithCondition(long shopId, string shopEmail, string shopName, int shopStatusId, int page) => ShopDAO.Instance.GetShopsWithCondition(shopId, shopEmail, shopName, shopStatusId, page);

        public Shop? GetShopDetail(long userId) => ShopDAO.Instance.GetShopDetail(userId);

		public Shop? GetMostPopularShop(string keyword)
		=> ShopDAO.Instance.GetMostPopularShop(keyword);

		public (long, List<Shop>) GetListShop(string keyword, int page)
		=> ShopDAO.Instance.GetListShop(keyword, page);
	}
}
