using BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTOs.Shop;

namespace DTOs.WishList
{
    public class WishListCustomerProductResponseDTO
    {
        public int TotalProduct { get; set; }
        public List<WishListCustomerProductDetailResponseDTO> Products { get; set; } = new List<WishListCustomerProductDetailResponseDTO>();
    }
}
