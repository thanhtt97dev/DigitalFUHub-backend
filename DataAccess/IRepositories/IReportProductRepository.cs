using BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.IRepositories
{
	public interface IReportProductRepository
	{
		void UpdateReportProduct(long reportProductId, int status, string note);
		void AddReportProduct(ReportProduct reportProduct);
		long GetNumberUnprocessedReportProducts();
		List<ReportProduct> GetReportProducts(string email, long productId, string productName, string shopName, DateTime? fromDate, DateTime? toDate, int reasonReportProduct, int reportProductStatusId, int page);
		int GetNumberReportProductByCondition(string email, long productId, string productName, string shopName, DateTime? fromDate, DateTime? toDate, int reasonReportProduct, int reportProductStatusId);
	}
}
