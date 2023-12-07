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
	public class ReportProductRepository : IReportProductRepository
	{
        public void AddReportProduct(ReportProduct reportProduct) => ReportProductDAO.Instance.AddReportProduct(reportProduct);

		public void UpdateReportProduct(long reportProductId, int status, string note) => ReportProductDAO.Instance.UpdateReportProduct(reportProductId, status, note);

		public long GetNumberUnprocessedReportProducts()
		=> ReportProductDAO.Instance.GetNumberUnprocessedReportProducts();
	}
}
