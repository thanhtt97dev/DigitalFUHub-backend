using BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.IRepositories
{
    public interface ICouponRepository
    {
        List<Coupon> GetCoupons (long shopId, string couponCode);
    }
}
