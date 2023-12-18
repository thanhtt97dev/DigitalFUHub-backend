using BusinessObject;
using BusinessObject.Entities;
using Comons;
using DTOs.MbBank;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Style;

namespace DataAccess.DAOs
{
	internal class ShopDAO
	{
		private static ShopDAO? instance;
		private static readonly object instanceLock = new object();
		public static ShopDAO Instance
		{
			get
			{
				lock (instanceLock)
				{
					if (instance == null)
					{
						instance = new ShopDAO();
					}
				}
				return instance;
			}
		}

		internal void AddShop(string avatarUrl, string shopName, long userId, string shopDescription)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				try
				{
					if (context.Shop.Any(x => x.UserId == userId)) throw new Exception("INVALID");
					if (context.Shop.Any(x => x.ShopName.ToLower() == shopName.ToLower())) throw new Exception("INVALID");

					//get fee
					var shopRegisterFeeDate = context.ShopRegisterFee.Max(x => x.StartDate);
					var shopRegisterFee = context.ShopRegisterFee.First(x => x.StartDate == shopRegisterFeeDate);

					Shop shop = new Shop()
					{
						DateCreate = DateTime.Now,
						ShopName = shopName,
						Avatar = avatarUrl,
						IsActive = true,
						Description = shopDescription,
						ShopRegisterFeeId = shopRegisterFee.ShopRegisterFeeId,
						UserId = userId,
					};
					context.Shop.Add(shop);

					User user = context.User.First(x => x.UserId == userId);
					if(user.AccountBalance - shopRegisterFee.Fee < 0)
					{
						throw new ArgumentOutOfRangeException(shopRegisterFee.Fee.ToString());
					}
					user.AccountBalance = user.AccountBalance - shopRegisterFee.Fee;
					user.RoleId = Constants.SELLER_ROLE;

					TransactionInternal transactionInternal = new TransactionInternal
					{
						UserId = userId,
						TransactionInternalTypeId = Constants.TRANSACTION_INTERNAL_TYPE_SELLER_REGISTRATION_FEE,
						PaymentAmount = shopRegisterFee.Fee,
						Note = "Register fee to become seller",
						DateCreate = DateTime.Now
					};

					context.TransactionInternal.Add(transactionInternal);

					context.SaveChanges();
				}
				catch (ArgumentOutOfRangeException e)
				{
					throw new Exception(e.Message);
				}
				catch (Exception e)
				{
					throw new Exception(e.Message);
				}

			}
		}

		internal bool IsExistShop(long userId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				return context.Shop.Any(x => x.UserId == userId);
			}
		}


		internal Product GetProductById(long productId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				Product product = context.Product.Where(x => x.ProductId == productId)
						.Select(x => new Product
						{
							ProductId = productId,
							CategoryId = x.CategoryId,
							Description = x.Description,
							Discount = x.Discount,
							ProductMedias = x.ProductMedias,
							ProductName = x.ProductName,
							ProductStatusId = x.ProductStatusId,
							Tags = x.Tags,
							Thumbnail = x.Thumbnail,
							ProductVariants = context.ProductVariant.Include(i => i.AssetInformations.Where(x => x.IsActive == true)).Where(x => x.ProductId == productId && x.isActivate == true).ToList(),
						}).First();
				return product;
			}
		}

		internal bool IsExistShopName(string shopName)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				return context.Shop.Any(x => x.ShopName.ToLower() == shopName.ToLower());
			}
		}

		internal Shop? GetShopById(long shopId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				return context.Shop.Include(_ => _.User).FirstOrDefault(x => x.UserId == shopId);
			}
		}

		internal void EditShop(Shop shopEdit)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				try
				{
					Shop? shop = context.Shop.FirstOrDefault(x => x.UserId == shopEdit.UserId);
					if (shop == null) throw new Exception("Not Found");
					shop.Description = shopEdit.Description;
					if (!string.IsNullOrEmpty(shopEdit.Avatar))
					{
						shop.Avatar = shopEdit.Avatar;
					}
					context.SaveChanges();
				}
				catch (Exception e)
				{
					throw new Exception(e.Message);
				}
			}
		}

		internal int GetNumberShopWithCondition(long shopId, string shopEmail, string shopName, int shopStatusId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var result = (from shop in context.Shop
							  join user in context.User
								 on shop.UserId equals user.UserId
							  where
							  ((shopId == 0) ? true : shop.UserId == shopId) &&
							  user.Email.Contains(shopEmail.Trim()) &&
							  shop.ShopName.Contains(shopName.Trim()) &&
							  ((shopId == 0) ? true : shop.IsActive == (shopStatusId == 1))
							  select new { }
							 ).Count();
				return result;
			}
		}

		internal List<Shop> GetShopsWithCondition(long shopId, string shopEmail, string shopName, int shopStatusId, int page)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var shops = (from shop in context.Shop
							 join user in context.User
								on shop.UserId equals user.UserId
							 where
							 ((shopId == 0) ? true : shop.UserId == shopId) &&
							 user.Email.Contains(shopEmail.Trim()) &&
							 shop.ShopName.Contains(shopName.Trim()) &&
							 ((shopStatusId == 0) ? true : shop.IsActive == (shopStatusId == 1))
							 select new Shop
							 {
								 UserId = shop.UserId,
								 ShopName = shop.ShopName,
								 Avatar = shop.Avatar,
								 DateCreate = shop.DateCreate,
								 IsActive = shop.IsActive,
								 User = new User
								 {
									 Email = user.Email,
									 TransactionInternals = (from transactionInternal in context.TransactionInternal
															 where transactionInternal.UserId == shop.UserId && transactionInternal.TransactionInternalTypeId == Constants.TRANSACTION_INTERNAL_TYPE_RECEIVE_PAYMENT
															 select new TransactionInternal
															 {
																 PaymentAmount = transactionInternal.PaymentAmount
															 }
															).ToList(),
								 },
								 Orders = (from order in context.Order
										   where order.ShopId == shop.UserId
										   select new Order
										   {
											   OrderStatusId = order.OrderStatusId,
										   }
										  ).ToList(),
								 Products = (from product in context.Product
											 where product.ShopId == shop.UserId
											 select new Product { }
										  ).ToList()
							 }
							 )
							 .Skip((page - 1) * Constants.PAGE_SIZE)
							 .Take(Constants.PAGE_SIZE)
							 .ToList();
				return shops;
			}
		}

		internal Shop? GetShopDetail(long userId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var result = (from shop in context.Shop
							  join user in context.User
							  on shop.UserId equals user.UserId
							  where shop.UserId == userId
							  select new Shop
							  {
								  UserId = shop.UserId,
								  Avatar = shop.Avatar,
								  ShopName = shop.ShopName,
								  DateCreate = shop.DateCreate,
								  Description = shop.Description,
								  IsActive = shop.IsActive,
								  User = new User
								  {
									  IsOnline = user.IsOnline,
									  LastTimeOnline = user.LastTimeOnline,
									  Username = user.Username,
								  },
								  Products = (from product in context.Product
											  where product.ShopId == shop.UserId
											  select new Product
											  {
												  TotalRatingStar = product.TotalRatingStar,
												  NumberFeedback = product.NumberFeedback,
											  }).ToList()

							  }).FirstOrDefault();

				return result;
			}
		}

		internal Shop? GetShopDetailAdmin(long userId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var result = (from shop in context.Shop
							  join user in context.User
							  on shop.UserId equals user.UserId
							  where shop.UserId == userId
							  select new Shop
							  {
								  UserId = shop.UserId,
								  Avatar = shop.Avatar,
								  ShopName = shop.ShopName,
								  DateCreate = shop.DateCreate,
								  DateBan = shop.DateBan,
								  Note = shop.Note,
								  Description = shop.Description,
								  IsActive = shop.IsActive,
								  User = new User
								  {
									  Email = user.Email,
									  IsOnline = user.IsOnline,
									  LastTimeOnline = user.LastTimeOnline,
									  TransactionInternals = (from transactionInternal in context.TransactionInternal
															  where transactionInternal.UserId == shop.UserId && transactionInternal.TransactionInternalTypeId == Constants.TRANSACTION_INTERNAL_TYPE_RECEIVE_PAYMENT
															  select new TransactionInternal
															  {
																  PaymentAmount = transactionInternal.PaymentAmount
															  }
															).ToList(),
								  },
								  Orders = (from order in context.Order
											where order.ShopId == shop.UserId
											select new Order
											{
												OrderStatusId = order.OrderStatusId,
											}
										  ).ToList(),
								  Products = (from product in context.Product
											  where product.ShopId == shop.UserId
											  select new Product
											  {
												  SoldCount = product.SoldCount,
												  TotalRatingStar = product.TotalRatingStar,
												  NumberFeedback = product.NumberFeedback,

											  }).ToList()

							  }).FirstOrDefault();

				return result;
			}
		}

		internal Shop? GetMostPopularShop(string keyword)
		{
			using (var context = new DatabaseContext())
			{
				string keywordSearch = keyword.Trim().ToLower();
				Shop? mostPopularShop = context.Shop.Include(x => x.User).Include(x => x.Products)
					.Where(x => (x.ShopName.ToLower().Contains(keywordSearch) || x.User.Username.ToLower().Contains(keywordSearch))
						&& x.IsActive == true && x.User.Status == true)
					.OrderByDescending(x => x.DateCreate)
					.FirstOrDefault();
				if (mostPopularShop == null)
				{
					return null;
				}
				return GetShopDetail(mostPopularShop.UserId);
			}
		}

		internal (long, List<Shop>) GetListShop(string keyword, int page)
		{
			using (var context = new DatabaseContext())
			{
				string keywordSearch = keyword.Trim().ToLower();
				var result = (from shop in context.Shop.Include(x => x.Products)
							  join user in context.User
							  on shop.UserId equals user.UserId
							  where (shop.ShopName.ToLower().Contains(keyword) || user.Username.Contains(keyword))
							  && shop.IsActive == true
							  && user.Status == true
							  select new Shop
							  {
								  UserId = shop.UserId,
								  Avatar = shop.Avatar,
								  ShopName = shop.ShopName,
								  DateCreate = shop.DateCreate,
								  Description = shop.Description,
								  User = new User
								  {
									  IsOnline = user.IsOnline,
									  LastTimeOnline = user.LastTimeOnline,
									  Username = user.Username
								  },
								  Products = (from product in context.Product
											  where product.ShopId == shop.UserId
											  &&
											  product.ProductStatusId == Constants.PRODUCT_STATUS_ACTIVE
											  ||
											  product.ProductStatusId == Constants.PRODUCT_STATUS_BAN
											  select new Product
											  {
												  TotalRatingStar = product.TotalRatingStar,
												  NumberFeedback = product.NumberFeedback,

											  }).ToList()

							  })
							  .OrderByDescending(x => x.DateCreate);

				return (result.LongCount(),
					result.Skip((page - 1) * Constants.PAGE_SIZE_SEARCH_SHOP)
					.Take(Constants.PAGE_SIZE_SEARCH_SHOP).ToList());

			}
		}

		internal void UpdateBanShop(long shopId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				try
				{
					Shop? shop = context.Shop.FirstOrDefault(x => x.UserId == shopId);
					if (shop == null) throw new Exception("Not Found");
					shop.DateBan = DateTime.Now;
					shop.IsActive = false;
					context.SaveChanges();
				}
				catch (Exception e)
				{
					throw new Exception(e.Message);
				}
			}
		}

		internal void UpdateShop(Shop shop)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				context.Shop.Update(shop);
				context.SaveChanges();
			}
		}
	}
}

