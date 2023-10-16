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



		internal void CreateShop(string shopName, long userId, string shopDescription)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				try
				{
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

		internal bool UserHasShop(long userId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				return context.Shop.Any(x => x.UserId == userId);
			}
		}

		internal bool ShopHasProduct(long userId, long productId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				return context.Shop.Include(i => i.Products)
					.Any(x => x.UserId == userId && x.Products.Any(x => x.ProductId == productId));
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

		internal bool CheckShopNameExisted(string shopName)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				return context.Shop.Any(x => x.ShopName.ToLower() == shopName.ToLower());
			}
		}
	}
}

