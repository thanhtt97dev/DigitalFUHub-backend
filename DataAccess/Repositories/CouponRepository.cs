using BusinessObject.Entities;
using DataAccess.DAOs;
using DataAccess.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
	public class CouponRepository : ICouponRepository
	{
		public void AddCoupon(Coupon coupon) => CouponDAO.Instance.AddCoupon(coupon);

		public bool CheckCouponCodeExist(string couponCode) => CouponDAO.Instance.CheckCouponCodeExist(couponCode);

		public List<Coupon> GetCoupons(long shopId, string couponCode) => CouponDAO.Instance.GetCoupons(shopId, couponCode);

		public Coupon? GetCoupons(long couponId) => CouponDAO.Instance.GetCoupon(couponId);

		public List<Coupon> GetListCouponsByShop(long userId, string couponCode, DateTime? startDate, DateTime? endDate, bool? isPublic)
		=> CouponDAO.Instance.GetListCouponsByShop(userId, couponCode, startDate, endDate, isPublic);

		public void UpdateCoupon(Coupon coupon) => CouponDAO.Instance.UpdateCoupon(coupon);
	}
}
