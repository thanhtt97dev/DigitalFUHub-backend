using BusinessObject.Entities;
using DataAccess.DAOs;
using DataAccess.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class WishListRepository : IWishListRepository
    {
        public void AddWishList(long productId, long userId) => WishListDAO.Instance.AddWishList(productId, userId);
        public (string, string, bool) CheckRequestWishListIsValid(long productId, long userId) => WishListDAO.Instance.CheckRequestWishListIsValid(productId, userId);
        public int GetNumberProductByConditions(long userId) => WishListDAO.Instance.GetNumberProductByConditions(userId);
        public List<Product> GetProductFromWishListByUserId(long userId, int page) => WishListDAO.Instance.GetProductFromWishListByUserId(userId, page);
        public bool IsExistWishList(long productId, long userId) => WishListDAO.Instance.IsExistWishList(productId, userId);
        public bool IsExistWishList(List<long> productIds, long userId) => WishListDAO.Instance.IsExistWishList(productIds, userId);
        public void RemoveWishList(long productId, long userId) => WishListDAO.Instance.RemoveWishList(productId, userId);
        public void RemoveWishListSelecteds(List<long> productIds, long userId) => WishListDAO.Instance.RemoveWishListSelecteds(productIds, userId);
    }
}
