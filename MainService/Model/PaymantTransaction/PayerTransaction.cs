using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainService.Model.PaymantTransaction
{
    public class PayerTransaction
    {
        public string Name { get; set; }
        public decimal Payment { get; set; }
        public DateTime Date { get; set; }
        public long Account_number { get; set; }
    }
}
