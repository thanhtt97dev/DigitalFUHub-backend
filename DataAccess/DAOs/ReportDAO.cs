using BusinessObject;
using BusinessObject.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;


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
        #endregion
    }
}
