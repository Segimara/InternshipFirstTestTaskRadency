using MainService.Extension;
using MainService.Handlers;
using MainService.Interfaces;
using MainService.Model;
using MainService.Model.PaymantTransaction;
using MainService.Services;
using Microsoft.Extensions.Configuration;

namespace MainService
{
    internal class Program
    {
        static int NumberOfProcessor = 25;
        static void Main(string[] args)
        {
            Console.WriteLine("Application Starting");
            IConfigurationRoot config = GetConfig();
            //Can swap to DI
            IFileHandlerFactory<PaymentTransaction> factory = new PaymentTransactionsFileHandlerFactory();
            IDailyLogHandler<DailyLog> dailyLogHandle = new DailyLogHandler();
            IFilePool filePool = new FileParallelPoolService(NumberOfProcessor);
            IFileProcessor<PaymentTransaction> fileProcessor = new PaymentTransactionsFileProcessorService(factory, dailyLogHandle).AddConfiguration(config.GetSection("FileHandler"));
            fileProcessor.Run();
        }
        
        static IConfigurationRoot GetConfig()
        {
            try
            {
                var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                return builder.Build();
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}