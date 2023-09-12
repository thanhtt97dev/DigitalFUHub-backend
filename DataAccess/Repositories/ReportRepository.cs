using BusinessObject;
using DataAccess.DAOs;
using DataAccess.IRepositories;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class ReportRepository : IReportRepository
    {
        public byte[] ReportUser(int id)
        {
            string workSheetName = "Report User";
            using ExcelPackage pack = new ExcelPackage();
            ExcelWorksheet ws = pack.Workbook.Worksheets.Add(workSheetName);

            IEnumerable<User> userList = ReportDAO.Instance.GetUsersReport(id);

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
