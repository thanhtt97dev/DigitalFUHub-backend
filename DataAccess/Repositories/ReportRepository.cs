using DataAccess.DAOs;
using DataAccess.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
	public class ReportRepository : IReportRepository
	{
		public Task<byte[]> GetReportUserInfoToExcel(int id) => ReportDAO.Instance.GetReportUserInfoToExcel(id);
	}
}
