﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrecoBot.DC
{
    public class TransactionDC
    {
        public long UserId { get; set; }
        public string TransactionId { get; set; }
        public string Pair { get; set; }
        public string Amount { get; set; }
        public DateTime DateTime { get; set; }
        /*public string CurrentCourse { get; set; }*/
    }
}
