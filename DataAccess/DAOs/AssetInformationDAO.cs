using BusinessObject;
using BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DAOs
{
    public class AssetInformationDAO
    {

        private static AssetInformationDAO? instance;
        private static readonly object instanceLock = new object();

        public static AssetInformationDAO Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new AssetInformationDAO();
                    }
                }
                return instance;
            }
        }

        internal List<AssetInformation> GetByProductVariantId (long productVariantId)
        {
            using (DatabaseContext context = new DatabaseContext())
            {
                var assetInformations = context.AssetInformation.Where(a => a.ProductVariantId == productVariantId).ToList();

                return assetInformations;
            }
        }
    }
}
