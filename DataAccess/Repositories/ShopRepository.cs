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
		public bool CheckShopNameExisted(string shopName) => ShopDAO.Instance.CheckShopNameExisted(shopName);


		public void CreateShop(string shopName, long userId, string shopDescription) => ShopDAO.Instance.CreateShop(shopName, userId, shopDescription);

		public Product GetProductById(long productId) =>  ShopDAO.Instance.GetProductById(productId);

		public Shop? GetShopById(long shopId) => ShopDAO.Instance.GetShopById(shopId);

		public bool ShopHasProduct(long userId, long productId) =>  ShopDAO.Instance.ShopHasProduct(userId, productId);

		public bool UserHasShop(long userId) =>  ShopDAO.Instance.UserHasShop(userId);
	}
}
