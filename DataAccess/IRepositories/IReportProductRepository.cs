using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.IRepositories
{
	public interface IReportProductRepository
	{
		void UpdateReportProduct(long reportProductId, int status, string note);
	}
}
