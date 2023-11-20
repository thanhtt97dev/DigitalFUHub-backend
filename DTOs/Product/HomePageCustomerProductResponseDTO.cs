using DTOs.Shop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Product
{
    public class HomePageCustomerProductResponseDTO
    {
        public int TotalProduct { get; set; }
        public List<HomePageCustomerProductDetailResponseDTO> Products { get; set; } = new List<HomePageCustomerProductDetailResponseDTO>();
    }
}
