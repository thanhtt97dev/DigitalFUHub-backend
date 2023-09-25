using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
    public class Category
    {

        public long CategoryId { get; set; }
        public string? CategoryName { get; set; }

        public virtual ICollection<Product>? Products { get; set; }
    }
}
