using BusinessObject;
using BusinessObject.Entities;
using Comons;
using DataAccess.IRepositories;
using DataAccess.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DAOs
{
    public class WishListDAO
    {
        private static WishListDAO? instance;
        private static readonly object instanceLock = new object();

        public static WishListDAO Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new WishListDAO();
                    }
                }
                return instance;
            }
        }

        internal WishList? GetWishListByUserId (long userId)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                var wishList = context.WishList.Where(x => x.UserId == userId);

                return null;
            }
        }

        internal bool IsExistWishList (long productId, long userId) {
            using (DatabaseContext context = new DatabaseContext())
            {
                var wishList = context.WishList.FirstOrDefault(x => x.UserId == userId && x.ProductId == productId);

                if (wishList == null) { return false; }
                else { return true; }
            }
        }

        internal void AddWishList (long productId, long userId)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                WishList newWishList = new WishList
                {
                    UserId = userId,
                    ProductId = productId
                };

                context.WishList.Add(newWishList);
                context.SaveChanges();
            }
        }

        internal void RemoveWishList(long productId, long userId)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                var wishList = context.WishList.FirstOrDefault(x => x.UserId == userId && x.ProductId == productId);
                if (wishList == null) throw new ArgumentNullException("wish list not found");

                context.WishList.Remove(wishList);
                context.SaveChanges();
            }
        }

        internal (string, string, bool) CheckRequestWishListIsValid (long productId, long userId)
        {
            string responseCode = "";
            string message = "";
            bool isOk = true;

            using (DatabaseContext context = new DatabaseContext())
            {
                if (productId == 0 || userId == 0)
                {
                    responseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
                    message = "Invalid";
                    isOk = false;
                    return (responseCode, message, isOk);
                }

                var user = context.User.FirstOrDefault(x => x.UserId == userId);

                if (user == null)
                {
                    responseCode = Constants.RESPONSE_CODE_DATA_NOT_FOUND;
                    message = "User not found";
                    isOk = false;
                    return (responseCode, message, isOk);
                }

                var product = context.Product.FirstOrDefault(x => x.ProductId == productId);

                if (product == null)
                {
                    responseCode = Constants.RESPONSE_CODE_DATA_NOT_FOUND;
                    message = "Product not found";
                    isOk = false;
                    return (responseCode, message, isOk);
                }

                responseCode = Constants.RESPONSE_CODE_SUCCESS;
                message = "Success";
                return (responseCode, message, isOk);
            }
           
        }


    }
}
