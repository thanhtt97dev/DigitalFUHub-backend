using BusinessObject.Entities;
using DataAccess.DAOs;
using DataAccess.IRepositories;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class ReportRepository : IReportRepository
    {
        public async Task<byte[]> ExportToExcel<T>(List<T> table, string filename)
        => await ReportDAO.Instance.ExportToExcel<T>(table, filename);

		public async Task<byte[]> ReportUser(int id)
        {
            string workSheetName = "Report User";
            using ExcelPackage pack = new ExcelPackage();
            ExcelWorksheet ws = pack.Workbook.Worksheets.Add(workSheetName);

            IEnumerable<User> userList = await ReportDAO.Instance.GetUsersReport(id);

            ws.Cells.LoadFromCollection(userList, true);

            var fileContents = pack.GetAsByteArray();

            if (fileContents == null || fileContents.Length == 0)
            {
                throw new ArgumentException("FileContents not found, Unable to create Excel file");
            }

            return fileContents;
        }
    }
}
