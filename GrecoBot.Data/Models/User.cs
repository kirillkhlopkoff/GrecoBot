using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrecoBot.Data.Models
{
    public class User
    {
        public long Id { get; set; }
        public string Phone { get; set; }
        public string ReferalCode { get; set; }
        public decimal SummOfTransactions { get; set; }
    }
}
