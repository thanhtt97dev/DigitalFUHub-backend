using BusinessObject;
using BusinessObject.Entities;
using DTOs.Shop;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

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



		internal void CreateShop(RegisterShopRequestDTO request)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				try
				{
					bool userShopExist = context.Shop.Any(x => x.UserId == request.UserId);
					if (userShopExist) throw new Exception("Đã tồn tại cửa hàng không thể tạo thêm.");
					Shop shop = new Shop()
					{
						DateCreate = DateTime.Now,
						ShopName = request.ShopName,
						IsActive = true,
						Balance = 0,
						Description = request.ShopDescription,
						UserId = request.UserId,
					};
					context.Shop.Add(shop);
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
							ProductVariants = context.ProductVariant.Include(i => i.AssetInformation.Where(x => x.IsActive == true)).Where(x => x.ProductId == productId && x.isActivate == true).ToList(),
						}).First();
				return product;
			}
		}
	}
}

