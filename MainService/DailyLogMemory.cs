using MainService.Model;
using System.Collections.Concurrent;
using System.Reflection.Metadata;

namespace MainService
{
    public class DailyLogMemory
    {
        private FileHandleResponse counterModel = new FileHandleResponse();
        private int ParsedFiles;
        private ConcurrentBag<string> _invalidFiles;
        private readonly object _lock = new object();
        public static DailyLogMemory Instance { get; } = new DailyLogMemory();
        public DailyLogMemory()
        {
            ParsedFiles = 0;
            counterModel.ParsedLines = 0;
            counterModel.InvalidLines = 0;
            _invalidFiles = new ConcurrentBag<string>();
        }
        public FileHandleResponse AddFileResponse(FileHandleResponse fileResponse)
        {
            lock (_lock)
            {
                counterModel.ParsedLines += fileResponse.ParsedLines;
                counterModel.InvalidLines += fileResponse.InvalidLines;
                ParsedFiles++;
                if (fileResponse.isInvalid)
                {
                    _invalidFiles.Add(fileResponse.FileName);
                }
                return counterModel;
            }
        }

        public DailyLog GetDailyLog()
        {
            var result = new DailyLog();
            lock (_lock)
            {
                result.InvalidFileNames = GetInvalidFiles();
                result.ParsedLines = counterModel.ParsedLines;
                result.InvalidLines = counterModel.InvalidLines;
                result.ParsedFiles = ParsedFiles;
                Reset();
                return result;
            }
        }

        public int AddParsedFiles(int amount)
        {
            lock (_lock)
            {
                return ParsedFiles += amount;
            }
        }
        public int AddParsedLines(int amount)
        {
            lock (_lock)
            {
                return counterModel.ParsedLines += amount;
            }
        }
        public int AddErrorsLines(int amount)
        {
            lock (_lock)
            {
                return counterModel.InvalidLines += amount;
            }
        }
        public void AddInvalidFiles(params string[] fileNames)
        {
            foreach (var fileName in fileNames)
            {
                _invalidFiles.Add(fileName);
            }
        }
        public int GetParsedFiles()
        {
            lock (_lock)
            {
                return ParsedFiles;
            }
        }
        public int GetParsedLines()
        {
            lock (_lock)
            {
                return counterModel.ParsedLines;
            }
        }
        public int GetInvalidLines()
        {
            lock (_lock)
            {
                return counterModel.InvalidLines;
            }
        }
        public int GetInvalidFilesCount()
        {
            lock (_lock)
            {
                return _invalidFiles.Count;
            }
        }
        public IList<string> GetInvalidFiles()
        {
            lock (_lock)
            {
                return _invalidFiles.ToList();
            }
        }
        public void Reset()
        {
            lock (_lock)
            {
                ParsedFiles = 0;
                counterModel.ParsedLines = 0;
                counterModel.InvalidLines = 0;
                _invalidFiles = new ConcurrentBag<string>();
            }
        }
        
    }
}
