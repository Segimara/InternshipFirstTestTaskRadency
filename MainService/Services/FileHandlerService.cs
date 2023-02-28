using MainService.Model;

namespace MainService.Services
{

    public class FileHandlerService
    {
        public static async Task<FileHandleResponse> HandleAsync(string filePath, string processedFilePath)
        {
            //Dictionary<string, Dictionary<string, List<Payment>>>()
            //Dictionary<string, Dictionary<string, List<Payment>>> fileData = new Dictionary<string, Dictionary<string, List<Payment>>>();
            FileHandleResponse fileHandleResponse = new FileHandleResponse();
            fileHandleResponse.isInvalid = true;
            fileHandleResponse.InvalidLines = 5;
            fileHandleResponse.FileName = Path.GetFileName(filePath);

            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    fileHandleResponse.ParsedLines++;
                }
            }
            var directory = Path.GetDirectoryName(processedFilePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            using (StreamWriter writer = new StreamWriter(processedFilePath))
            {
                writer.WriteAsync(fileHandleResponse.FileName);
            }
            return fileHandleResponse;
        }
    }
}
