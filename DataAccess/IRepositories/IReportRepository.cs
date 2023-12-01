using BusinessObject;
using DTOs.Seller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.IRepositories
{
    public interface IReportRepository
    {
        Task<byte[]> ReportUser(int id);
        Task<byte[]> ExportToExcel<T>(List<T> table, string filename, DateTime? fromDate, DateTime? toDate, string shopName, int status);
    }
}
