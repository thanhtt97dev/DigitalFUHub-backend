using BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.IRepositories
{
    public interface IReasonReportProductRepository
    {
        List<ReasonReportProduct> GetAll();
        ReasonReportProduct? GetEntityById(int reasonReportProductId);
    }
}
