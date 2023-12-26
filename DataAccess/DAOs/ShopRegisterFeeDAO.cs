using BusinessObject;
using BusinessObject.Entities;
using DTOs.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DAOs
{
	public class ShopRegisterFeeDAO
	{
		private static ShopRegisterFeeDAO? instance;
		private static readonly object instanceLock = new object();

		public static ShopRegisterFeeDAO Instance
		{
			get
			{
				lock (instanceLock)
				{
					if (instance == null)
					{
						instance = new ShopRegisterFeeDAO();
					}
				}
				return instance;
			}
		}

		internal List<ShopRegisterFee> GetFees(long shopRegisterFeeId, int maxFee, DateTime? fromDate, DateTime? toDate)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var fees = (from shopRegisterFee in context.ShopRegisterFee
							where (1 == 1) &&
							(shopRegisterFeeId != 0 ? shopRegisterFee.ShopRegisterFeeId == shopRegisterFeeId : true) &&
							((fromDate != null && toDate != null) ? fromDate <= shopRegisterFee.StartDate && toDate >= shopRegisterFee.StartDate : true)
							select new ShopRegisterFee
							{
								ShopRegisterFeeId = shopRegisterFee.ShopRegisterFeeId,
								Fee = shopRegisterFee.Fee,
								StartDate = shopRegisterFee.StartDate,
								EndDate = shopRegisterFee.EndDate,
								Shops = context.Shop.Where(x => x.ShopRegisterFeeId == shopRegisterFee.ShopRegisterFeeId).ToList(),
							})
							.OrderByDescending(x => x.ShopRegisterFeeId)
							.ToList();
				return fees;
			}
		}

		internal void AddNewShopRegisterFee(long fee)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var transaction = context.Database.BeginTransaction();
				try
				{
					var shopRegisterFeeId = context.ShopRegisterFee.Max(x => x.ShopRegisterFeeId);
					var shopRegisterFeeOld = context.ShopRegisterFee.First(x => x.ShopRegisterFeeId == shopRegisterFeeId);
					shopRegisterFeeOld.EndDate = DateTime.Now;

					var ShopRegisterFee = new ShopRegisterFee
					{
						StartDate = DateTime.Now,
						Fee = fee,
					};
					context.ShopRegisterFee.Add(ShopRegisterFee);
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

		internal long GetCurrentFee()
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var shopRegisterFeeDate = context.ShopRegisterFee.Max(x => x.StartDate);
				var shopRegisterFee = context.ShopRegisterFee.First(x => x.StartDate == shopRegisterFeeDate);
				return shopRegisterFee.Fee;
			}
		}
	}
}
