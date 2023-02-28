using System.Collections.Concurrent;

namespace MainService
{
    public class DailyLogMemory
    {
        volatile int _parsedFiles;
        volatile int _parsedLines;
        volatile int _errorsLines;
        private ConcurrentBag<string> _invalidFiles;
        private readonly object _lock = new object();
        public DailyLogMemory()
        {
            _parsedFiles = 0;
            _parsedLines = 0;
            _errorsLines = 0;
            _invalidFiles = new ConcurrentBag<string>();
        }
        public int AddParsedFiles(int amount)
        {
            lock (_lock)
            {
                return _parsedFiles += amount;
            }
        }
        public int AddParsedLines(int amount)
        {
            lock (_lock)
            {
                return _parsedLines += amount;
            }
        }
        public int AddErrorsLines(int amount)
        {
            lock (_lock)
            {
                return _errorsLines += amount;
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
                return _parsedFiles;
            }
        }
        public int GetParsedLines()
        {
            lock (_lock)
            {
                return _parsedLines;
            }
        }
        public int GetErrorsLines()
        {
            lock (_lock)
            {
                return _errorsLines;
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
                _parsedFiles = 0;
                _parsedLines = 0;
                _errorsLines = 0;
                _invalidFiles = new ConcurrentBag<string>();
            }
        }
        
        public static DailyLogMemory Instance { get; } = new DailyLogMemory();
    }
}
