using BusinessObject;
using BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DAOs
{
	public class CouponDAO
	{

		private static CouponDAO? instance;
		private static readonly object instanceLock = new object();

		public static CouponDAO Instance
		{
			get
			{
				lock (instanceLock)
				{
					if (instance == null)
					{
						instance = new CouponDAO();
					}
				}
				return instance;
			}
		}

		internal List<Coupon> GetCoupons(long shopId, string couponCode)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var coupons = context.Coupon.Where(c => c.ShopId == shopId
															&& c.IsActive == true
															&& c.CouponCode.Equals(couponCode)
															&& c.EndDate > DateTime.Now).ToList();

				return coupons;
			}
		}

		internal List<Coupon> GetListCouponsByShop(long userId, string couponCode, DateTime? startDate, DateTime? endDate, bool? isPublic)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				List<Coupon> coupons = context.Coupon
					.Where(c => c.ShopId == userId
							&& c.IsActive == true
							&& c.CouponCode.ToLower().Contains(couponCode.ToLower())
							&& (startDate == null ? true : c.StartDate.Date >= startDate.Value.Date)
							&& (endDate == null ? true : c.EndDate.Date <= endDate.Value.Date)
							&& (isPublic == null ? true : c.IsPublic == isPublic)
							).ToList();

				return coupons;
			}
		}

		internal void AddCoupon(Coupon coupon)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				try
				{
					context.Coupon.Add(coupon);
					context.SaveChanges();
				}
				catch (Exception e)
				{
					throw new Exception(e.Message);
				}
			}
		}

		internal Coupon? GetCoupon(long couponId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				return context.Coupon.FirstOrDefault(x => x.CouponId == couponId);
			}
		}

		internal void UpdateCoupon(Coupon coupon)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				try
				{
					context.Coupon.Update(coupon);
					context.SaveChanges();
				}
				catch (Exception e)
				{
					throw new Exception(e.Message);
				}
			}
		}

		internal bool CheckCouponCodeExist(string couponCode)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				return context.Coupon.Any(x => x.CouponCode.ToLower() == couponCode.ToLower());
			}
		}
	}
}
