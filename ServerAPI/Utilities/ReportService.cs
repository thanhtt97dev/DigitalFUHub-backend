using BusinessObject;
using DTOs;
using Microsoft.Data.SqlClient;
using Microsoft.OData.Edm;
using OfficeOpenXml;
using System.Data;

namespace ServerAPI.Utilities
{
    public class ReportService
    {
        private readonly string _connectionString;

        public ReportService()
        {
            _connectionString = "server=localhost;database=DBTest;uid=sa;pwd=sa;Integrated security=true;TrustServerCertificate=true";
        }
        public byte[] ExportUserToExcel(int id)
        {
            string reportname = $"export_user_{Guid.NewGuid():N}.xlsx";
            using ExcelPackage pack = new ExcelPackage();
            ExcelWorksheet ws = pack.Workbook.Worksheets.Add(reportname);
            //List<SqlParameter> listParams = new List<SqlParameter> { 
            //    new SqlParameter
            //    {
            //        ParameterName = "@id",
            //        SqlDbType = SqlDbType.Int,
            //        Value = id,
            //        Direction = ParameterDirection.Input,
            //    }
            //};
            IDictionary<string, object> listParams = new Dictionary<string, object>();
            listParams.Add("@id", id);

            SqlDataReader sdr = CallProcedureService<User>.Instance.GetData("getByQuery", listParams);

            List<string> demo = new List<string>();

                while (sdr.Read())
                {
                    // Đọc dữ liệu từ các cột và xử lý nó
                    // Tên cột và giá trị có thể thay đổi động
                    for (int i = 0; i < sdr.FieldCount; i++)
                    {
                        string columnName = sdr.GetName(i);
                        object columnValue = sdr[i];
                    demo.Add(columnName);

                        // Xử lý dữ liệu ở đây
                    }
                }
            


            ws.Cells.LoadFromCollection(new List<string>{ "hieu", "a"}, true);

            return pack.GetAsByteArray();
        }
    }
}
