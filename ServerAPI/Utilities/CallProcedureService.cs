using Microsoft.Data.SqlClient;
using System.Data;

namespace ServerAPI.Utilities
{
    public class CallProcedureService<T>
    {
        private readonly string? _connectionString;

        private static CallProcedureService<T>? instance;
        private static readonly object instanceLock = new object();

        public static CallProcedureService<T> Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new CallProcedureService<T>();
                    }
                }
                return instance;
            }
        }

        public CallProcedureService()
        {
            _connectionString = "server=localhost;database=DBTest;uid=sa;pwd=sa;Integrated security=true;TrustServerCertificate=true";
        }

        public SqlDataReader GetData(string nameProcedure, IDictionary<string, object> listParams)
        {


            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Tạo SqlCommand để gọi stored procedure
                using (SqlCommand cmd = new SqlCommand(nameProcedure, connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Thêm tham số đầu vào nếu cần


                    // Thêm tham số đầu ra để chứa kết quả của stored procedure
                    foreach (var item in listParams)
                    {
                        cmd.Parameters.AddWithValue(item.Key, item.Value);
                    }
                    //SqlParameter outputParameter = new SqlParameter("@Result", SqlDbType.NVarChar, 500);
                    //outputParameter.Direction = ParameterDirection.Output;
                    //cmd.Parameters.Add(outputParameter);

                    // Thực thi stored procedure
                    SqlDataReader sdr = cmd.ExecuteReader();

                    // Lấy kết quả từ tham số đầu ra
                    //string result = outputParameter.Value.ToString();

                    // Đọc và xử lý kết quả dưới đây
                    // Sử dụng SqlDataReader để đọc dữ liệu
                    //Dictionary<string, object> listData = new Dictionary<string, object>();


                    //using (SqlDataReader reader = cmd.ExecuteReader())
                    //{
                    //    while (reader.Read())
                    //    {
                    //        // Đọc dữ liệu từ các cột và xử lý nó
                    //        // Tên cột và giá trị có thể thay đổi động
                    //        for (int i = 0; i < reader.FieldCount; i++)
                    //        {
                    //            string columnName = reader.GetName(i);
                    //            object columnValue = reader[i];
                    //            listData.Add(columnName, columnValue);

                    //            // Xử lý dữ liệu ở đây
                    //        }
                    //    }
                    //}
                    return sdr;
                }

            }

           
        }

    }
}
