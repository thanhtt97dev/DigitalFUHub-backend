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

		internal async Task<byte[]> ExportToExcel<T>(List<T> table, string filename)
		{
			using ExcelPackage pack = new ExcelPackage();
			ExcelWorksheet ws = pack.Workbook.Worksheets.Add(filename);
			ws.Cells["A1"].LoadFromCollection(table, true, TableStyles.Light1);
			return await pack.GetAsByteArrayAsync();
		}
		#endregion
	}
}
