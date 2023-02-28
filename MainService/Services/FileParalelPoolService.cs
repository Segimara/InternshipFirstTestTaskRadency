using MainService.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainService.Services
{
    //todo create dispose pattern and ask "would app process remaining {N} files?"
    public class FileParalelPoolService : IFilePool
    {
        private readonly int _batchSize = 100;
        private readonly DailyLogMemory _DailyLogMemoryStorege;
        

        
        private readonly ConcurrentQueue<(string, string)> fileQueue = new ConcurrentQueue<(string, string)>();

        public FileParalelPoolService(int NumberOfProcessor, DailyLogMemory DailyLogMemoryStorege)
        {
            _batchSize = NumberOfProcessor;
            _DailyLogMemoryStorege = DailyLogMemoryStorege;
        }
        
        private void ProcessFileBatch()
        {
            var tasks = new List<Task>();
            
            while (fileQueue.TryDequeue(out (string, string) Paths))
            {
                var task = Task.Run(async () =>
                {
                    //string subFolderPath = @"\" + DateTime.Now.ToString("MM-dd-yyyy");
                    string subFolderPath = @"\" + DateTime.Now.ToString("hh_mm");
                    var fileDestributionPath = Path.Combine(Paths.Item2 + subFolderPath, UniqueFileNumber.Instance.GetUniqueNumber() + ".txt");
                    var fileResponse = await FileHandlerService.HandleAsync(Paths.Item1, fileDestributionPath);
                    _DailyLogMemoryStorege.AddParsedLines(fileResponse.ParsedLines);
                    if (fileResponse.isInvalid)
                    {
                        _DailyLogMemoryStorege.AddErrorsLines(fileResponse.InvalidLines);
                        _DailyLogMemoryStorege.AddInvalidFiles(fileResponse.FileName);
                    }

                });
                
                tasks.Add(task);
                
                if (tasks.Count >= Environment.ProcessorCount)
                {
                    Task.WaitAll(tasks.ToArray());
                    GC.Collect();
                }
            }
            
            Task.WaitAll(tasks.ToArray());
            GC.Collect();
        }

        public void AddFile(string filePath, string processedFolderPath)
        {
            fileQueue.Enqueue((filePath, processedFolderPath));
            
            if (fileQueue.Count <= _batchSize)
            {
                ProcessFileBatch();
            }
        }
    }

}
