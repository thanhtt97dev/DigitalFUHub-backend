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

		public bool IsExistCouponCode(long shopId, string couponCode) => CouponDAO.Instance.IsExistCouponCode(shopId, couponCode);

        public Coupon? GetCouponByCode(string couponCode) => CouponDAO.Instance.GetCouponByCode(couponCode);

        public List<Coupon> GetCouponPublic (long shopId) => CouponDAO.Instance.GetCouponPublic(shopId);

		public Coupon? GetCoupon(long couponId, long shopId) => CouponDAO.Instance.GetCoupon(couponId, shopId);

		public List<Coupon> GetListCouponsByShop(long userId, string couponCode, DateTime? startDate, DateTime? endDate, bool? isPublic)
		=> CouponDAO.Instance.GetListCouponsByShop(userId, couponCode, startDate, endDate, isPublic);

		public void UpdateStatusCoupon(Coupon coupon) => CouponDAO.Instance.UpdateStatusCoupon(coupon);

		public void UpdateCoupon(Coupon coupon) => CouponDAO.Instance.UpdateCoupon(coupon);
	}
}
