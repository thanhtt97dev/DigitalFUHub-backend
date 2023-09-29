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

	

		internal async Task CreateShopAsync(RegisterShopRequestDTO request)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				try
				{
					bool userShopExist = await context.Shop.AnyAsync(x => x.UserId == request.UserId);
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
					await context.SaveChangesAsync();
				}
				catch (Exception e)
				{

					throw new Exception(e.Message);
				}
				
			}
		}

		internal async Task<bool> UserHasShopAsync(long userId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				return await context.Shop.AnyAsync(x => x.UserId == userId);
			}
		}

		internal async Task<bool> ShopHasProductAsync(long userId, long productId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				return await context.Shop.Include(i => i.Products)
					.AnyAsync(x => x.UserId == userId && x.Products.Any(x => x.ProductId == productId));
			}
		}

		internal async Task<Product> GetProductByIdAsync(long productId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				Product product = await context.Product.Where(x => x.ProductId == productId)
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
						}).FirstAsync();
				return product;
			}
		}
	}
}
