using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Bank
{
    public class CreateTransactionRequestDTO
    {
        public long UserId { get; set; }
        public long Amount { get; set; }
    }
}
