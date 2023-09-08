using BusinessObject;
using DTOs;
using Microsoft.Data.SqlClient;
using Microsoft.OData.Edm;
using OfficeOpenXml;
using System.Data;
using System.Reflection;

namespace ServerAPI.Utilities
{
	public class ReportService
	{

		private readonly string? _connectionString;
		public ReportService()
		{
			_connectionString = "server=localhost;database=DBTest;uid=sa;pwd=sa;Integrated security=true;TrustServerCertificate=true";
		}

		#region Get data from procedure
		public List<object?> GetData(string nameProcedure, IDictionary<string, object> listParams, Type objectResultType)
		{
			List<object?> data = new List<object?>();
			using (SqlConnection connection = new SqlConnection(_connectionString))
			{
				connection.Open();
				using (SqlCommand cmd = new SqlCommand(nameProcedure, connection))
				{
					cmd.CommandType = CommandType.StoredProcedure;
					foreach (var item in listParams)
					{
						cmd.Parameters.AddWithValue(item.Key, item.Value);
					}

					SqlDataReader reader = cmd.ExecuteReader();
					while (reader.Read())
					{
						object? instanceObject = Activator.CreateInstance(objectResultType);
						int s = reader.FieldCount;
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

		public byte[] ExportUserToExcel(int id)
		{
			string workSheetName = "Report User";
			using ExcelPackage pack = new ExcelPackage();
			ExcelWorksheet ws = pack.Workbook.Worksheets.Add(workSheetName);

			IDictionary<string, object> listParams = new Dictionary<string, object>() { };
			listParams.Add("@id", id);

			List<object?> data = GetData("getByQuery", listParams, typeof(User));
			List<User> userList = data.Cast<User>().ToList();

			ws.Cells.LoadFromCollection(userList, true);

			return pack.GetAsByteArray();
		}
	}
}
