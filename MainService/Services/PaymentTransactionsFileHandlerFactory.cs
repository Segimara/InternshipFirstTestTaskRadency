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
        public IFileHandler<PaymentTransaction> this[string key]
        {
            get
            {
                return Parsers[key];
            }
        }

        public static IParser<PaymentTransaction> Parser { get; set; } = new PaymentTransactionParser();



        public IDictionary<string, IFileHandler<PaymentTransaction>> Parsers { get; private set; } = new Dictionary<string, IFileHandler<PaymentTransaction>>{
            { ".csv", new CSVFileHandlerService(Parser) },
            { ".txt", new TXTFileHandlerService(Parser) },
        };
        
    }
}
