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
        public List<Product> GetProductFromWishListByUserId(long userId) => WishListDAO.Instance.GetProductFromWishListByUserId(userId);
        public bool IsExistWishList(long productId, long userId) => WishListDAO.Instance.IsExistWishList(productId, userId);
        public void RemoveWishList(long productId, long userId) => WishListDAO.Instance.RemoveWishList(productId, userId);
    }
}
