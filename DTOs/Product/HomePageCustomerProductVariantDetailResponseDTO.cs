﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Product
{
    public class HomePageCustomerProductVariantDetailResponseDTO
    {
        public long ProductVariantId { get; set; }
        public int Discount { get; set; }
        public long Price { get; set; }
    }
}
