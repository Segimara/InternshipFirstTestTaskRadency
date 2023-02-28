using MainService.Interfaces;
using MainService.Model;
using MainService.Model.PaymantTransaction;
using MainService.Services;
using System.IO;
using System.Text.Json;
namespace MainService.Handlers
{

    public class CSVFileHandlerService 
    {
        public IParser<PaymentTransaction> Parser { get; set; }
        public CSVFileHandlerService(IParser<PaymentTransaction> parser)
        {
            Parser = parser;
        }
        public FileHandleResponse Handle(string filePath, string destributionPath)
        {
            FileHandleResponse response = new FileHandleResponse();
            response.FileName = Path.GetFileName(filePath);
            var lines = File.ReadLines(filePath).Skip(1);
            var serialized = lines.Select(line =>
            {
                PaymentTransaction parsedLine = Parser.Parse(line);
                response.ParsedLines++;
                if (parsedLine == null)
                {
                    response.InvalidLines++;
                    response.isInvalid = true;
                }
                return parsedLine;
            });
            var transformed = serialized.Where(n => n != null).paymentToCityTransactions();
            using (var writer = new StreamWriter(File.Open(filePath, FileMode.Create)))
            {
                JsonSerializer.Serialize(writer.BaseStream, transformed);

            }
            File.Delete(filePath);
            return response;
        }
    }
}
