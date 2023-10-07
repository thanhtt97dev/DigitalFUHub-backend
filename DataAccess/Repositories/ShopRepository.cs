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
		public  void CreateShop (RegisterShopRequestDTO request) =>  ShopDAO.Instance.CreateShop(request);

		public Product GetProductById(long productId) =>  ShopDAO.Instance.GetProductById(productId);

        public bool ShopHasProduct(long userId, long productId) =>  ShopDAO.Instance.ShopHasProduct(userId, productId);

		public bool UserHasShop(long userId) =>  ShopDAO.Instance.UserHasShop(userId);
	}
}
