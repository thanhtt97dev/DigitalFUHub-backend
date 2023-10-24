using BusinessObject.Entities;
using BusinessObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs.Cart;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.DAOs
{
    public class CartDAO
    {

        private static CartDAO? instance;
        private static readonly object instanceLock = new object();

        public static CartDAO Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new CartDAO();
                    }
                }
                return instance;
            }
        }

        internal void AddProductToCart(long userId, long shopId, long productVariantId, int quantity)
		{
            using (DatabaseContext context = new DatabaseContext())
            {
                var transaction = context.Database.BeginTransaction();
                try
                {
					var cart = context.Cart
				   .Include(x => x.CartDetails)
				   .FirstOrDefault(x => x.UserId == userId && x.ShopId == shopId);

					//Check Cart of user with shop existed
					if (cart == null)
					{
						cart = new Cart
						{
							UserId = userId,
							ShopId = shopId,
						};
						context.Cart.Add(cart);
                        context.SaveChanges();
					}

                    if(cart.CartDetails == null || cart.CartDetails.Count == 0)
                    {
                        CartDetail cartDetail = new CartDetail
                        {
                            CartId = cart.CartId,
                            ProductVariantId = productVariantId,    
                            Quantity = quantity
                        };
                        context.CartDetail.Add(cartDetail);
                        context.SaveChanges();
                    }
                    else
                    {
                        bool productVariantExistedInCart = cart.CartDetails.Any(x => x.ProductVariantId == productVariantId);
                        if (productVariantExistedInCart)
                        {
                            var cartDetailId = cart.CartDetails
                                .First(x => x.ProductVariantId == productVariantId)
                                .CartDetailId;
                            var cartDetail = context.CartDetail.First(x => x.CartDetailId == cartDetailId);
                            cartDetail.Quantity = cartDetail.Quantity + quantity;
                            context.SaveChanges();
                        }
                        else
                        {
                            CartDetail cartDetail = new CartDetail
                            {
                                CartId = cart.CartId,   
                                ProductVariantId = productVariantId,
                                Quantity = quantity
                            };
                            context.CartDetail.Add(cartDetail);
                            context.SaveChanges();
                        }
					}
					context.SaveChanges();
					transaction.Commit();
				}
				catch (Exception ex) 
                {
                    transaction.Rollback();
                    throw new Exception(ex.Message);
                }
			}

		}


        internal Cart? GetCart(long userId, long productVariantId)
        {
            return null;
            using (DatabaseContext context = new DatabaseContext())
            {
                /*
                var cart = context.Cart.FirstOrDefault(
                        c => c.UserId == userId && c.ProductVariantId == productVariantId
                    );

                return cart;
                */
            }
        }

        internal List<Cart> GetCartsByUserId(long userId)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                var result = (from cart in context.Cart
                             join shop in context.Shop
                                on cart.ShopId equals shop.UserId
                             where cart.UserId == userId
                             select new Cart
                             {
                                 CartId = cart.CartId,
                                 Shop = new Shop
                                 {
                                     UserId = shop.UserId,
                                     ShopName = shop.ShopName,
                                 },
                                 CartDetails = (from cartDetail in context.CartDetail
                                                where cartDetail.CartId == cart.CartId
                                                select new CartDetail
                                                {
                                                    CartDetailId = cartDetail.CartDetailId,   
                                                    ProductVariant = (from productVariant in context.ProductVariant
                                                                     where productVariant.ProductVariantId == cartDetail.ProductVariantId
                                                                     select new ProductVariant 
                                                                     {
                                                                         ProductVariantId = productVariant.ProductVariantId,
                                                                         Name = productVariant.Name,
                                                                         Price = productVariant.Price,
                                                                         isActivate = productVariant.isActivate,
                                                                         Product = (from product in context.Product
                                                                                   where product.ProductId == productVariant.ProductId
                                                                                   select new Product
                                                                                   {
                                                                                       ProductId = product.ProductId,
                                                                                       ProductName = product.ProductName,
                                                                                       Discount = product.Discount,
                                                                                       Thumbnail = product.Thumbnail,
                                                                                       ProductStatusId = product.ProductStatusId,
																				   }).First(),
                                                                     }).First(),
													Quantity = cartDetail.Quantity, 
                                                }
                                               ).ToList()

                             }).ToList();
                return result;
			}
		}

		 /*
	     internal (bool, long) CheckQuantityForCart(long userId, long productVariantId, long quantity)
	     {
		     return (true, 0);
		     using (DatabaseContext context = new DatabaseContext())
		     {

			     var cart = GetCart(userId, productVariantId);
			     if (cart != null)
			     {
				     long quantityPurchased = quantity + cart.Quantity;
				     long quantityProductVariant = context.AssetInformation.Count(a => a.ProductVariantId == productVariantId);
				     if (quantityPurchased > quantityProductVariant)
				     {
					     return (false, cart.Quantity);
				     }
			     }
			     return (true, cart?.Quantity ?? 0);
			
	        }
        }
        */


        internal async Task DeleteCart(long userId, long productVariantId)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                /*
                var cart = await context.Cart.FirstOrDefaultAsync(
                        c => c.UserId == userId && c.ProductVariantId == productVariantId
                    );
                if (cart == null) throw new ArgumentNullException("Not found cart");
                context.Cart.Remove(cart);
                context.SaveChanges();
                */
            }
        }

        internal (bool, int) CheckValidQuantityAddProductToCart(long userId, long shopId, long productVariantId, int quantity)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                // get cart detail
                var cart = context.Cart
                    .FirstOrDefault(x => x.UserId == userId && x.ShopId == shopId);
                // get number asset infomation with productVariantId is activate
                var numberAssetInfomation = context.AssetInformation.Count(x => x.ProductVariantId == productVariantId && x.IsActive);

                int totalQuantity = quantity;
                if(cart != null) 
                {
                    // get cart detail with productVariantId
                    var cartDetail = context.CartDetail.FirstOrDefault(x => x.ProductVariantId == productVariantId);
                    if (cartDetail != null) totalQuantity += cartDetail.Quantity;
				}
                if(totalQuantity > numberAssetInfomation)
                {
                    return (false, numberAssetInfomation);
                }
			}
			return (true,0);
        }

		internal bool CheckProductVariantInShop(long shopId, long productVariantId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				// check productVariant in shop
				bool isProductVariantInShop = context.ProductVariant
					.Include(x => x.Product)
					.Any(x => x.ProductVariantId == productVariantId && x.Product.ShopId == shopId);

				return isProductVariantInShop;
			}
		}

		internal CartDetail? GetCartDetail(long cartDetailId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
                return context.CartDetail.FirstOrDefault(x => x.CartDetailId == cartDetailId);
			}
		}

		internal void UpdateQuantityCartDetail(long cartDetailId, int quantity)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var cartDetail = context.CartDetail.FirstOrDefault(x => x.CartDetailId == cartDetailId);
                if (cartDetail == null) return;
                cartDetail.Quantity = quantity;
                context.CartDetail.Update(cartDetail);
                context.SaveChanges();
			}
		}

		internal void RemoveCartDetail(long cartDetailId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var cartDetail = context.CartDetail.FirstOrDefault(x => x.CartDetailId == cartDetailId);
				if (cartDetail == null) return;
                var cart = context.Cart
                    .Include(x => x.CartDetails)
                    .Select(x => new Cart { CartId = x.CartId, CartDetails = x.CartDetails })
                    .First(x => x.CartId == cartDetail.CartId);
                if(cart.CartDetails.Count() == 1)
                {
                    context.CartDetail.Remove(cartDetail);
                    context.Cart.Remove(cart);
                }
                else
                {
					context.CartDetail.Remove(cartDetail);
				}
                context.SaveChanges();
			}
		}
	}
}

