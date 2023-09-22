using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs.Bank
{
    public class DepositTransactionResponeDTO
    {
        public string Code { get; set; } = string.Empty;
        public long Amount { get; set; }
    }
}
