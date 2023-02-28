using MainService.Interfaces;
using MainService.Model.PaymantTransaction;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MainService.Parsers
{
    public class PaymentTransactionParser : IParser<PaymentTransaction>
    {
        public PaymentTransaction Parse(string line)
        {
            //creatre regex for mathches all comas are not in a quotation marks
            var lines = line.Split(",");
            
            PaymentTransaction paymentTransaction = new PaymentTransaction();
            try
            {
                paymentTransaction.FirstName = lines[0].Trim();
                paymentTransaction.LastName = lines[1].Trim();
                paymentTransaction.Address = lines[2].Replace("“", "").Trim() + ", "+ lines[3].Trim() + ", " + lines[4].Replace("”", "").Trim();
                NumberStyles style = NumberStyles.AllowDecimalPoint;
                CultureInfo culture = CultureInfo.InvariantCulture; 
                paymentTransaction.Payment = decimal.Parse(lines[5].Trim(), style, culture);
                paymentTransaction.Date = DateTime.ParseExact(lines[6].Trim(), "yyyy-dd-MM", CultureInfo.InvariantCulture);
                paymentTransaction.AccountNumber = long.Parse(lines[7].Trim());
                paymentTransaction.Service = lines[8].Trim();
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
