using BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.IRepositories
{
    public interface IWishListRepository
    {
        bool IsExistWishList(long productId, long userId);
        bool IsExistWishList(List<long> productIds, long userId);
        void AddWishList(long productId, long userId);
        void RemoveWishList(long productId, long userId);
        (string, string, bool) CheckRequestWishListIsValid(long productId, long userId);
        List<Product> GetProductFromWishListByUserId(long userId);
        void RemoveWishListSelecteds(List<long> productIds, long userId);
    }
}
