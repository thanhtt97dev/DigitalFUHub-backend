using DTOs.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Shop
{
    public class ShopDetailCustomerProductResponseDTO
    {
        public int TotalPage { get; set; }
        public int TotalProduct { get; set; }
        public List<ShopDetailCustomerProductDetailResponseDTO> Products { get; set; } = new List<ShopDetailCustomerProductDetailResponseDTO>();
    }
}
