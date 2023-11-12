using BusinessObject.Entities;
using DataAccess.DAOs;
using DataAccess.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class ReasonReportProductRepository : IReasonReportProductRepository
    {
        public List<ReasonReportProduct> GetAll() => ReasonReportProductDAO.Instance.GetAll();
        public ReasonReportProduct? GetEntityById(int reasonReportProductId) => ReasonReportProductDAO.Instance.GetEntityById(reasonReportProductId);
    }
}
