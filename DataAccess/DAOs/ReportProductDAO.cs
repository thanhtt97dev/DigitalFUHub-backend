using BusinessObject.Entities;
using BusinessObject;
using DTOs.MbBank;
using DTOs.Seller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Comons;

namespace DataAccess.DAOs
{
	internal class ReportProductDAO
	{
		private static ReportProductDAO? instance;
		private static readonly object instanceLock = new object();

		public static ReportProductDAO Instance
		{
			get
			{
				lock (instanceLock)
				{
					if (instance == null)
					{
						instance = new ReportProductDAO();
					}
				}
				return instance;
			}
		}

		internal void UpdateReportProduct(long reportProductId, int status, string note)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var reportProduct = context.ReportProduct.FirstOrDefault(x => x.ReportProductId == reportProductId);
				if (reportProduct == null) throw new Exception("Data not found");
				reportProduct.ReportProductStatusId = status;
				reportProduct.Note = note;
				context.ReportProduct.Update(reportProduct);
				context.SaveChanges();
			}
		}

		internal void AddReportProduct (ReportProduct reportProduct)
		{
            using (DatabaseContext context = new DatabaseContext())
            {
                context.ReportProduct.Add(reportProduct);
                context.SaveChanges();
            }
        }

		internal long GetNumberUnprocessedReportProducts()
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				return context.ReportProduct.LongCount(x => x.ReportProductStatusId == Constants.REPORT_PRODUCT_STATUS_VERIFYING);
			}
		}
	}
}
