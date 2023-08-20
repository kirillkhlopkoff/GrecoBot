using GrecoBot.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrecoBot.DC
{
    public class TransactionInfoDC
    {
        public string TransactionId { get; set; }
        public string Pair { get; set; }
        public decimal Amount { get; set; }
        public DateTime DateTime { get; set; }
        public StatusTransaction StatusTransaction { get; set; }
    }
}
