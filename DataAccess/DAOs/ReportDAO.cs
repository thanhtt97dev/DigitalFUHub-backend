using BusinessObject;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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

		#region Get data from procedure
		private async Task<List<object?>> GetData(string nameProcedure, IDictionary<string, object> listParams, Type objectResultType)
		{
			List<object?> data = new List<object?>();
			using (ApiContext context = new ApiContext())
			{
				SqlConnection connection = new SqlConnection(context.connectionString);
				connection.Open();
				using (SqlCommand cmd = new SqlCommand(nameProcedure, connection))
				{
					cmd.CommandType = CommandType.StoredProcedure;
					foreach (var item in listParams)
					{
						cmd.Parameters.AddWithValue(item.Key, item.Value);
					}

					SqlDataReader reader = await cmd.ExecuteReaderAsync();
					while (reader.Read())
					{
						object? instanceObject = Activator.CreateInstance(objectResultType);
						for (int i = 0; i < reader.FieldCount; i++)
						{
							string columnName = reader.GetName(i);
							object columnValue = reader[i];
							PropertyInfo? property = objectResultType.GetProperty(columnName);
							if (property != null) property.SetValue(instanceObject, columnValue, null);
						}
						data.Add(instanceObject);
					}
				}
				connection.Close();
				return data;
			}
		}
		#endregion

		internal async Task<byte[]> GetReportUserInfoToExcel(int id)
		{
			string workSheetName = "Report User";
			using ExcelPackage pack = new ExcelPackage();
			ExcelWorksheet ws = pack.Workbook.Worksheets.Add(workSheetName);

			IDictionary<string, object> listParams = new Dictionary<string, object>() { };
			listParams.Add("@id", id);

			List<object?> data = await GetData("getByQuery", listParams, typeof(User));
			List<User> userList = data.Cast<User>().ToList();

			ws.Cells.LoadFromCollection(userList, true);

			return pack.GetAsByteArray();
		}
	}
}
