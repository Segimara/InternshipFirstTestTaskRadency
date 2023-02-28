using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainService.Model.PaymantTransaction
{
    public class CityTransaction
    {
        public string City { get; set; }
        public IEnumerable<ServiceTransaction> Services { get; set; }
        public decimal Total { get; set; }
    }
}
