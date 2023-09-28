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
	}
}
