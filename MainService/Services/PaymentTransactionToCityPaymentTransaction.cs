using MainService.Model.PaymantTransaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace MainService.Services
{
    public static class PaymentTransactionToCityPaymentTransaction
    {
        public static IEnumerable<CityTransaction> paymentToCityTransactions(this IEnumerable<PaymentTransaction> transactions)
        {
            return transactions
                    .GroupBy(t => t.Address.Split(',')[0].Trim()) // group by city (assuming address is in format "street, city")
                    .Select(g => new CityTransaction {
                        City = g.Key,
                        Services = g.GroupBy(t => t.Service) // group by service
                                    .Select(s => new ServiceTransaction
                                    {
                                        Name = s.Key,
                                        Payers = s.Select(p => new PayerTransaction
                                        {
                                            Name = $"{p.FirstName} {p.LastName}",
                                            Payment = p.Payment,
                                            Date = p.Date,
                                            Account_number = p.AccountNumber
                                        }),
                                        Total = s.Sum(p => p.Payment)
                                    }),
                        Total = g.Sum(t => t.Payment) // total payments for city
                    });
        }
    }
}
