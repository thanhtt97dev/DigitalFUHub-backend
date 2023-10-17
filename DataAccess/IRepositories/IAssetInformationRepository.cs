using BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.IRepositories
{
    public interface IAssetInformationRepository
    {
        List<AssetInformation> GetByProductVariantId(long productVariantId);
        int GetQuantityAssetInformationProductVariantAvailable(long productVariantId);  

	}
}
