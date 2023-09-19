using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject
{
	public class Category
	{
		public Category()
		{
			Products = new List<Product>();
		}

		public long CategoryId { get; set; }
        public string? CategoryName{ get; set; }

		public virtual ICollection<Product> Products { get; set;}
    }
}
