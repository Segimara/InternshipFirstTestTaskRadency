using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainService.Model.PaymantTransaction
{
    public class ServiceTransaction
    {
        public string Name { get; set; }
        public IEnumerable<PayerTransaction> Payers { get; set; }
        public decimal Total { get; set; }
    }
}
