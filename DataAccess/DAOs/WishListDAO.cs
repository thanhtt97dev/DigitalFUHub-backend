using BusinessObject;
using BusinessObject.Entities;
using Comons;
using DataAccess.IRepositories;
using DataAccess.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        internal List<Product> GetProductFromWishListByUserId (long userId, int page)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                // product id of user in wish list
                var productIds = context.WishList.Where(x => x.UserId == userId).Select(x => x.ProductId);

                var products = (from product in context.Product
                                join shop in context.Shop
                                on product.ShopId equals shop.UserId
                             where productIds.Contains(product.ProductId)
                             select new Product
                             {
                                 ProductId = product.ProductId,
                                 ProductName = product.ProductName,
                                 Thumbnail = product.Thumbnail,
                                 TotalRatingStar = product.TotalRatingStar,
                                 NumberFeedback = product.NumberFeedback,
                                 SoldCount = product.SoldCount,
                                 ProductStatusId = product.ProductStatusId,
                                 Shop = new Shop { IsActive = shop.IsActive},
                                 ProductVariants = (from productVariant in context.ProductVariant
                                                    where productVariant.ProductId == product.ProductId
                                                    select new ProductVariant
                                                    {
                                                        ProductVariantId = productVariant.ProductId,
                                                        Discount = productVariant.Discount,
                                                        Price = productVariant.Price,
                                                        AssetInformations = (from assetInformation in context.AssetInformation
                                                                             where assetInformation.ProductVariantId == productVariant.ProductVariantId
                                                                             &&
                                                                             assetInformation.IsActive
                                                                             select new AssetInformation { }).ToList()
                                                    }).ToList()
                             }
                                ).Skip((page - 1) * Constants.PAGE_SIZE_PRODUCT_WISH_LIST)
                                         .Take(Constants.PAGE_SIZE_PRODUCT_WISH_LIST)
                                         .ToList();

              
                return products;
            }
        }


        internal int GetNumberProductByConditions(long userId)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                // product id of user in wish list
                var productIds = context.WishList.Where(x => x.UserId == userId).Select(x => x.ProductId);

                return (from product in context.Product
                        join shop in context.Shop
                        on product.ShopId equals shop.UserId
                        where productIds.Contains(product.ProductId)
                        select new Product {}).Count();
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

        internal bool IsExistWishList(List<long> productIds, long userId)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                var resultIsExistWishList = context.WishList.Any(x => x.UserId == userId && productIds.Contains(x.ProductId));

                return resultIsExistWishList;
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

				// update number like count number of product
				var product = context.Product.First(x => x.ProductId == productId);
                product.LikeCount += 1;
                context.Product.Update(product);

                context.SaveChanges();
            }
        }

        internal void RemoveWishList(long productId, long userId)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                var wishList = context.WishList.First(x => x.UserId == userId && x.ProductId == productId);
                context.WishList.Remove(wishList);

				// update number like count number of product
				var product = context.Product.First(x => x.ProductId == productId);
				product.LikeCount -= 1;
				context.Product.Update(product);

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

                // check own product
                var shop = context.Shop.FirstOrDefault(x => x.UserId == userId);
                if (shop != null)
                {
                    var products = context.Product.Where(x => x.ShopId == shop.UserId).ToList();
                    if (products.Count > 0)
                    {
                        var productExisted = products.FirstOrDefault(x => x.ProductId == productId);
                        if (productExisted != null)
                        {
                            responseCode = Constants.RESPONSE_CODE_NOT_ACCEPT;
                            message = "Can't do it with your own product!";
                            isOk = false;
                            return (responseCode, message, isOk);
                        }
                    }
                }

                isOk = true;
                responseCode = Constants.RESPONSE_CODE_SUCCESS;
                message = "Success";
                return (responseCode, message, isOk);
            }
           
        }

        internal void RemoveWishListSelecteds(List<long> productIds, long userId)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                var wishLists = context.WishList.Where(x => x.UserId == userId && productIds.Contains(x.ProductId)).ToList();
                
                if (wishLists.Count > 0)
                {
                    foreach (var productId in productIds)
                    {
						// update number like count number of product
						var product = context.Product.First(x => x.ProductId == productId);
						product.LikeCount -= 1;
						context.Product.Update(product);
					}

                    context.WishList.RemoveRange(wishLists);
                    context.SaveChanges();
                }
            }
        }


    }
}
