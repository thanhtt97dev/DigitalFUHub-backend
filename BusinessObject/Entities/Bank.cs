using BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObject.Entities
{
    public class Bank
    {
        [Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long BankId { get; set; }
        public string BankCode { get; set; } = null!;
		public string BankName { get; set; } = string.Empty;
        public bool isActivate { get; set; }
		public virtual ICollection<UserBank>? UserBanks { get; set; }
	}
}
