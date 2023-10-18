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
    public class AssetInformationRepository : IAssetInformationRepository
    {
        public List<AssetInformation> GetByProductVariantId(long productVariantId) 
            => AssetInformationDAO.Instance.GetByProductVariantId(productVariantId);

        public int GetQuantityAssetInformationProductVariantAvailable(long productVariantId) => AssetInformationDAO.Instance.GetQuantityAssetInformationProductVariantAvailable(productVariantId);

	}
}
