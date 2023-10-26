using BusinessObject;
using BusinessObject.Entities;
using Comons;
using Microsoft.EntityFrameworkCore;


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



		internal void AddShop(string shopName, long userId, string shopDescription)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				try
				{
					if (context.Shop.Any(x => x.UserId == userId)) throw new Exception("INVALID");
					if (context.Shop.Any(x => x.ShopName.ToLower() == shopName.ToLower())) throw new Exception("INVALID");
					Shop shop = new Shop()
					{
						DateCreate = DateTime.Now,
						ShopName = shopName,
						IsActive = true,
						Description = shopDescription,
						UserId = userId,
					};
					context.Shop.Add(shop);
					User user = context.User.First(x => x.UserId == userId);
					user.RoleId = Constants.SELLER_ROLE;
					context.SaveChanges();
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
				return context.Shop.FirstOrDefault(x => x.UserId == shopId);
			}
		}
	}
}

