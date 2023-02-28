using MainService.Handlers;
using MainService.Interfaces;
using MainService.Model.PaymantTransaction;
using MainService.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainService.Services
{
    public class PaymentTransactionsFileHandlerFactory : IFileHandlerFactory<PaymentTransaction>
    {
        public static IParser<PaymentTransaction> Parser { get; set; } = new PaymentTransactionParser();
        
        public static IDictionary<string, Type> Parsers { get; private set; } = new Dictionary<string, Type>{
            { ".csv", CSVFileHandlerService.Handle()},
            { ".txt", TXTFileHandlerService},
        };
    }
}
