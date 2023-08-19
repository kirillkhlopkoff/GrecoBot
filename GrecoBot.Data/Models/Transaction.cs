using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrecoBot.Data.Models
{
    public class Transaction
    {
        [ForeignKey("User")]
        public long UserId { get; set; }
        public string TransactionId { get; set; }
        public string Pair { get; set; }
        public string Amount { get; set; }
        public DateTime DateTime { get; set; }
        public string CurrentCourse { get; set; }
        public User User { get; set; }
    }
}
