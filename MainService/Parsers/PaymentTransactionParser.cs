using MainService.Interfaces;
using MainService.Model.PaymantTransaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainService.Parsers
{
    public class PaymentTransactionParser : IParser<PaymentTransaction>
    {
        public PaymentTransaction Parse(string line)
        {
            var lines = line.Split(new char[] { ',' });
            PaymentTransaction paymentTransaction = new PaymentTransaction();
            try
            {
                paymentTransaction.FirstName = lines[0];
                paymentTransaction.LastName = lines[1];
                paymentTransaction.Address = lines[2];
                paymentTransaction.Payment = decimal.Parse(lines[3]);
                paymentTransaction.Date = DateTime.Parse(lines[4]);
                paymentTransaction.AccountNumber = long.Parse(lines[5]);
                paymentTransaction.Service = lines[6];
                return paymentTransaction;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "\non line: ");
                Console.WriteLine(line);
                return null;
            } 
            
        }
        
    }
}
