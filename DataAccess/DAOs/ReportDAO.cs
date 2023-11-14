using BusinessObject;
using BusinessObject.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.Table;
using OfficeOpenXml;
using OfficeOpenXml.Style;

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

		internal async Task<byte[]> ExportToExcel<T>(List<T> table, string filename, DateTime? fromDate, DateTime? toDate, string shopName)
		{
			using ExcelPackage pack = new ExcelPackage();
			ExcelWorksheet ws = pack.Workbook.Worksheets.Add(filename);
			using (ExcelRange rangeMerge = ws.Cells["A1:H1"])
			{
				string title = $"BÁO CÁO DANH SÁCH CÁC ĐƠN HÀNG \n " +
					$"{(fromDate != null && toDate != null ? $"({fromDate.Value.ToString("dd/MM/yyyy")} - {toDate.Value.ToString("dd/MM/yyyy")})" : "")}";
				rangeMerge.Merge = true;
				rangeMerge.Value = title;
				rangeMerge.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
				rangeMerge.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
				rangeMerge.Style.Font.Bold = true;
				rangeMerge.Style.Font.Size = 16f;
				rangeMerge.EntireRow.Height = 44;
			}
			using (ExcelRange rangeMerge = ws.Cells["A2:H2"])
			{
				rangeMerge.Merge = true;
				rangeMerge.Value = $"Tên cửa hàng: {shopName}";
				rangeMerge.Style.Font.Size = 12f;
				rangeMerge.Style.Font.Bold = true;
			}
			ExcelRangeBase lsTable = ws.Cells["A3"].LoadFromCollection(table, true, TableStyles.Light1);
			lsTable.AutoFitColumns();
			lsTable.EntireColumn.AutoFit();
			lsTable.Style.Font.Size = 12f;
			return await pack.GetAsByteArrayAsync();
		}
		#endregion
	}
}
