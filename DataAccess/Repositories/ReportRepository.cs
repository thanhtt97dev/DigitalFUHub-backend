using DataAccess.DAOs;
using DataAccess.IRepositories;

namespace DataAccess.Repositories
{
	public class ReportRepository : IReportRepository
	{
		public Task<byte[]> GetReportUserInfoToExcel(int id) => ReportDAO.Instance.GetReportUserInfoToExcel(id);
	}
}
