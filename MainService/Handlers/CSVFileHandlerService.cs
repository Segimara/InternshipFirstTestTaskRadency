using MainService.Interfaces;
using MainService.Model;
using MainService.Model.PaymantTransaction;
using MainService.Services;
using Newtonsoft.Json;
using System.Text.Json;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace MainService.Handlers
{

    public class CSVFileHandlerService : IFileHandler<PaymentTransaction>
    {
        public IParser<PaymentTransaction> Parser { get; set; }
        public CSVFileHandlerService(IParser<PaymentTransaction> parser)
        {
            Parser = parser;
        }
        public FileHandleResponse Handle(string filePath, string destributionPath, string fileName)
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
            var directoryNAme = destributionPath.Replace(fileName, "");
            if (!Directory.Exists(directoryNAme))
                Directory.CreateDirectory(directoryNAme);
            using (var stream = new StreamWriter(File.Open(destributionPath, FileMode.Create)))
            {
                stream.Write(JsonConvert.SerializeObject(transformed));

            }
            FileInfo fileInfo = new FileInfo(filePath);
            fileInfo.Delete();
            return response;
        }
    }
}
