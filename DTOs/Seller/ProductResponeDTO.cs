﻿using BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Seller
{
	public class ProductResponeDTO
	{
		public long ProductId { get; set; }
		public string? ProductName { get; set; }
		public string? Thumbnail { get; set; }
        public string? Description { get; set; }
        public int Discount { get; set; }
        public ProductStatus? ProductStatus { get; set; }
        public Shop? Shop { get; set; }
        public Category? Category { get; set; }
        public List<ProductVariantResponeDTO>? ProductVariants { get; set; }
	}
}

