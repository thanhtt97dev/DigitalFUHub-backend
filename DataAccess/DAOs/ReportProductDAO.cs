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

		internal List<ReportProduct> GetReportProducts(string email, long productId, string productName, string shopName, DateTime? fromDate, DateTime? toDate, int reasonReportProductId, int reportProductStatusId, int page)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var reportProducts = (from reportProduct in context.ReportProduct	
									 join user in context.User on reportProduct.UserId equals user.UserId	
									 join product in context.Product on reportProduct.ProductId equals product.ProductId
									 join shop in context.Shop on product.ShopId equals shop.UserId
									 join reasonReportProduct in context.ReasonReportProduct on reportProduct.ReasonReportProductId equals reasonReportProduct.ReasonReportProductId
									 where	
									 user.Email.Contains(email) &&
									 ((productId == 0) ? true : product.ProductId == productId) &&
									 product.ProductName.Contains(productName) && 
									 shop.ShopName.Contains(shopName) &&
									 ((fromDate != null && toDate != null) ? fromDate <= reportProduct.DateCreate && toDate >= reportProduct.DateCreate : true) &&
									 ((reasonReportProductId == 0) ? true : reportProduct.ReasonReportProductId == reasonReportProductId) &&
									 ((reportProductStatusId == Constants.REPORT_PRODUCT_STATUS_ALL) ? true : reportProduct.ReportProductStatusId == reportProductStatusId)
									 select new ReportProduct
									 {
										 ReportProductId = reportProduct.ReportProductId,	
										 User = new User
										 {
											 UserId = user.UserId,
											 Email = user.Email
										 },
										 Product = new Product
										 {
											 ProductId = product.ProductId,
											 ProductName = product.ProductName,
											 Thumbnail = product.Thumbnail,
											 Shop = new Shop 
											 {
												 UserId = shop.UserId,
												 ShopName = shop.ShopName,
											 }
										 },
										 ReasonReportProduct = new ReasonReportProduct
										 {
											 ReasonReportProductId = reasonReportProduct.ReasonReportProductId,
											 ViName = reasonReportProduct.ViName,
											 ViExplanation = reasonReportProduct.ViExplanation
										 },
										 DateCreate = reportProduct.DateCreate,
										 Description = reportProduct.Description,	
										 Note = reportProduct.Note,
										 ReasonReportProductId = reportProduct.ReasonReportProductId,
										 ReportProductStatusId = reportProduct.ReportProductStatusId,

									 })
									.OrderByDescending(x => x.DateCreate)
									.Skip((page - 1) * Constants.PAGE_SIZE)
									.Take(Constants.PAGE_SIZE)
									.ToList();
				return reportProducts;
			}
		}

		internal int GetNumberReportProductByCondition(string email, long productId, string productName, string shopName, DateTime? fromDate, DateTime? toDate, int reasonReportProductId, int reportProductStatusId)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				var reportProducts = (from reportProduct in context.ReportProduct
									  join user in context.User on reportProduct.UserId equals user.UserId
									  join product in context.Product on reportProduct.ProductId equals product.ProductId
									  join shop in context.Shop on product.ShopId equals shop.UserId
									  where
									  user.Email.Contains(email) &&
									  ((productId == 0) ? true : product.ProductId == productId) &&
									  product.ProductName.Contains(productName) &&
									  shop.ShopName.Contains(shopName) &&
									  ((fromDate != null && toDate != null) ? fromDate <= reportProduct.DateCreate && toDate >= reportProduct.DateCreate : true) &&
									  ((reasonReportProductId == 0) ? true : reportProduct.ReasonReportProductId == reasonReportProductId) &&
									  ((reportProductStatusId == Constants.REPORT_PRODUCT_STATUS_ALL) ? true : reportProduct.ReportProductStatusId == reportProductStatusId)
									  select new {}
									 ).Count();
				return reportProducts;
			}
		}
	}
}
