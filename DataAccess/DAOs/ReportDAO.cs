using BusinessObject;
using BusinessObject.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Table;
using OfficeOpenXml;

namespace DataAccess.DAOs
{
	internal class ReportDAO
	{
		private static ReportDAO? instance;
		private static readonly object instanceLock = new object();

		public static ReportDAO Instance
		{
			get
			{
				lock (instanceLock)
				{
					if (instance == null)
					{
						instance = new ReportDAO();
					}
				}
				return instance;
			}
		}


		#region Get Users Report (sample)
		public async Task<List<User>> GetUsersReport(int id)
		{
			using (DatabaseContext context = new DatabaseContext())
			{
				string sql = "EXECUTE dbo.getUserReport @id";
				List<User> result = await context.User.FromSqlRaw(sql,
						new SqlParameter("@id", id)
					).ToListAsync();

				return result;
			}

		}

		internal async Task<byte[]> ExportToExcel<T>(List<T> table, string filename, DateTime? fromDate, DateTime? toDate)
		{
			using ExcelPackage pack = new ExcelPackage();
			ExcelWorksheet ws = pack.Workbook.Worksheets.Add(filename);
			using (ExcelRange rangeMerge = ws.Cells["A1:H1"])
			{
				string title = $"BÁO CÁO DANH SÁCH CÁC ĐƠN HÀNG \n " +
					$"{(fromDate != null && toDate != null ? $"({fromDate.Value.ToString("dd/MM/yyyy")} - {toDate.Value.ToString("dd/MM/yyyy")})" :"")}";
				rangeMerge.Merge = true;
				rangeMerge.Value = title;
				rangeMerge.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
				rangeMerge.Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
			}
			ws.Cells["A2"].LoadFromCollection(table, true, TableStyles.Light1);
			return await pack.GetAsByteArrayAsync();
		}
		#endregion
	}
}
