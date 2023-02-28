using MainService.Extension;
using MainService.Interfaces;
using MainService.Services;
using Microsoft.Extensions.Configuration;

namespace MainService
{
    internal class Program
    {
        static int NumberOfProcessor = 100;
        static void Main(string[] args)
        {
            Console.WriteLine("Application Starting");
            IConfigurationRoot config = GetConfig();
            FileProcessorConfig fileHandelerConfig = config.GetSection("FileHandler").Get<FileProcessorConfig>();
            //NumberOfProcessor = config.GetSection("NumberOfProcessor").Get<int>();
            //Use DI
            //var host = new HostBuilder()
            //    .ConfigureServices((hostContext, services) =>
            //    {
            //        services.AddHostedService<Worker>();
            //    });
            IFileProcessor fileProcessor = new FileProcessorService().AddConfiguration(config.GetSection("FileHandler"));
            fileProcessor.Run();
        }
        
        static IConfigurationRoot GetConfig()
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            return builder.Build();
        }
    }
}