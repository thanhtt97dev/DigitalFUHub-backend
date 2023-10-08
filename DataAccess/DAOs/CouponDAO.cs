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

        internal List<Coupon> GetByShopId(long shopId)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                var coupons = context.Coupon.Where(c => c.ShopId == shopId 
                                                            && c.IsActive == true 
                                                            && c.Quantity > 0
                                                            && c.EndDate > DateTime.Now).ToList();

                return coupons;
            }
        }
    }
}
