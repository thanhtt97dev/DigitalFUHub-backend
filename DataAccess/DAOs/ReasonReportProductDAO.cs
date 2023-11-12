using BusinessObject;
using BusinessObject.Entities;
using DTOs.MbBank;
using DTOs.Seller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DAOs
{
    public class ReasonReportProductDAO
    {
        private static ReasonReportProductDAO? instance;
        private static readonly object instanceLock = new object();

        public static ReasonReportProductDAO Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new ReasonReportProductDAO();
                    }
                }
                return instance;
            }
        }

        internal List<ReasonReportProduct> GetAll ()
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                var reasons = context.ReasonReportProduct.ToList();

                return reasons;
            }
        }

        internal ReasonReportProduct? GetEntityById(int reasonReportProductId)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                var reason = context.ReasonReportProduct.FirstOrDefault(x => x.ReasonReportProductId == reasonReportProductId);

                return reason;
            }
        }
    }
}
