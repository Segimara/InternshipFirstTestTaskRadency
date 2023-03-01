using MainService.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Timers;
using MainService.Extension;
using System.Net.Http.Headers;
using MainService.Model;
using MainService.Model.PaymantTransaction;
using System.Linq;

namespace MainService.Services
{
    //decompose to IFileProcessor, IFileCheker
    public class PaymentTransactionsFileProcessorService : IFileProcessor<PaymentTransaction>, IDisposable
    {
        private int maxThreads;
        bool isWorking = false;
        //private DailyLogMemory DailyLogMemory.Insrance; //todo extract interface
        private FileProcessorConfig fileHandlerConfig;
        //IFilePool _poolService;
        FileSystemWatcher watcher;
        System.Timers.Timer timer;
        IFileHandlerFactory<PaymentTransaction> _factory;
        private IFileHandler<PaymentTransaction> _fileHandler;
        private IDailyLogHandler<DailyLog> _dailyLogHandle;
        private bool disposed = false;
        //public IFilePool TaskPool { get => _poolService; }

        public PaymentTransactionsFileProcessorService()
        {
            watcher = new FileSystemWatcher();
        }
        //add injection of IParserFactory 
        public PaymentTransactionsFileProcessorService(/*DailyLogMemory DailyLogMemory.Insrance,*/
            IFileHandlerFactory<PaymentTransaction> factory,
            IDailyLogHandler<DailyLog> dailyLogHandle) : this()
        {
            //this.DailyLogMemory.Insrance = DailyLogMemory.Insrance;
            this.fileHandlerConfig = fileHandlerConfig;
            _factory = factory;
            _dailyLogHandle = dailyLogHandle;
            //_poolService = filePool;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Created -= OnFileCreated;
                watcher.Dispose();

                timer.Stop();
                timer.Elapsed -= OnTimedEvent;
                timer.Dispose();
                
            }

            disposed = true;
        }


        ~PaymentTransactionsFileProcessorService()
        {
            Dispose(false);
        }

        public void Run()
        {
            string command = "start";
            bool work = true;
            while (work)
            {
                switch (command.ToLower())
                {
                    case "start":
                        {
                            Start();
                            break;
                        }
                    case "reset":
                        {
                            Reset();
                            break;
                        }
                    case "stop":
                        {
                            Stop();
                            if (isWorking = false)
                                work = false;
                            break;
                        }
                    case "exit":
                        {
                            ForceStop();
                            work = false;
                            break;
                        }
                    default:
                        {
                            Console.WriteLine("Invalid command");
                            break;
                        }
                }
                command = Console.ReadLine();
            }
        }

        private void ForceStop()
        {
            Stop();
        }
        //public PaymentTransactionsFileProcessorService SetupDailyStore(DailyLogMemory DailyLogMemory.Insrance)
        //{
        //    this.DailyLogMemory.Insrance = DailyLogMemory.Insrance;
        //    return this;
        //}
        public PaymentTransactionsFileProcessorService SetupConfiguration(FileProcessorConfig config)
        {
            this.fileHandlerConfig = config;
            return this;
        }
        public PaymentTransactionsFileProcessorService SetupDailyStore(FileProcessorConfig config)
        {
            this.fileHandlerConfig = config;
            return this;
        }
        public PaymentTransactionsFileProcessorService SetupParserFactory(IFileHandlerFactory<PaymentTransaction> factory)
        {
            _factory = factory;
            return this;
        }
        public PaymentTransactionsFileProcessorService SetupFileHandler(IFileHandler<PaymentTransaction> fileHandler)
        {
            _fileHandler = fileHandler;
            return this;
        }
        public PaymentTransactionsFileProcessorService SetupDailyLogHandle(IDailyLogHandler<DailyLog> dailyLogHandle)
        {
            _dailyLogHandle = dailyLogHandle;
            return this;
        }
        public void Start()
        {
            Console.WriteLine("Starting an application");
            isWorking = true;
            CheckConfig();
            InitFolders(fileHandlerConfig);
            StartFileSystemWatcher();
            SetupTimer();
            Console.WriteLine("Application was started");
            AddExistingFiles();
        }
        public void Stop()
        {

            Console.WriteLine("Start pausing an application");
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
            timer.Stop();
            timer.Dispose();
            Console.WriteLine("Application was stopped");
            isWorking = false;
        }
        public void Reset()
        {
            Stop();
            Start();
        }
        void InitFolders(FileProcessorConfig configuration)
        {
            if (!Directory.Exists(configuration.FolderA))
            {
                Directory.CreateDirectory(configuration.FolderA);
            }
            if (!Directory.Exists(configuration.FolderB))
            {
                Directory.CreateDirectory(configuration.FolderB);
            }
        }
        void AddExistingFiles()
        {
            var files = new List<string>();
            //files = Directory.GetFiles(Path.GetFullPath(fileHandlerConfig.FolderA), "*.txt").ToList();
            foreach (var fileType in _factory.Parsers.Keys)
            {
                files.AddRange(Directory.GetFiles(Path.GetFullPath(fileHandlerConfig.FolderA), $"*{fileType}"));
            }
            foreach (var file in files)
            {
                HandleFile(file);
            }
        }

        void HandleFile(string filePath)
        {
            var file = new FileInfo(filePath);
            if (_factory.Parsers.Keys.Any(x => x == "." + Path.GetExtension(filePath)))
            {
                return;
            }
            var handler = _factory[file.Extension];
            string subFolderPath = DateTime.Now.ToString("dd_MM_yyyy");
            //string subFolderPath = DateTime.Now.ToString("hh");
            string subfolderCPath = Path.Combine(fileHandlerConfig.FolderB, subFolderPath);
            var filename = "output" + UniqueFileNumber.Instance.GetUniqueNumber() + ".json";
            string fileDest = Path.Combine(subfolderCPath, filename);
            DailyLogMemory.Instance.AddFileResponse(handler.Handle(filePath, fileDest, filename));
        }
        void StartFileSystemWatcher()
        {
            // Create a new FileSystemWatcher and set its properties.
            watcher.Path = fileHandlerConfig.FolderA;
            watcher.Filter = "*";
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.IncludeSubdirectories = false;

            watcher.Created += OnFileCreated;
            watcher.Error += OnWatcherError;

            watcher.IncludeSubdirectories = false;
            watcher.EnableRaisingEvents = true;
        }

        private async void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            try
            {
                HandleFile(e.FullPath);
            }
            catch(Exception ee)
            {
                Console.WriteLine(ee.Message);
            }
        }

        private void OnWatcherError(object sender, ErrorEventArgs e)
        {
            Console.WriteLine(e.GetException().Message);
        }
        
        void SetupTimer()
        {
            DateTime now = DateTime.Now;
            DateTime midnight = DateTime.Today.AddDays(1).AddTicks(-1);
            //TimeSpan timeUntilMidnight = midnight - now;
            
            TimeSpan timeUntilMidnight = TimeSpan.FromMinutes(1);
            timer = new System.Timers.Timer(timeUntilMidnight.TotalMilliseconds);
            timer.AutoReset = false;
            timer.Elapsed += OnTimedEvent;
            timer.Enabled = true;
        }
        private void OnTimedEvent(object? sender, ElapsedEventArgs e)
        {
            OnTimerElapsed();
        }
        private void OnTimerElapsed()
        {
            if (DailyLogMemory.Instance.GetParsedLines() == 0 && DailyLogMemory.Instance.GetParsedFiles() == 0 && DailyLogMemory.Instance.GetInvalidLines() == 0)
            {
                SetupTimer();
                return;
            }

            var fileName = Path.Combine(fileHandlerConfig.FolderB, DateTime.Now.ToString("dd_MM_yyyy"), "meta.log");
            
            var dailyLog = DailyLogMemory.Instance.GetDailyLog();
            _dailyLogHandle.Handle(dailyLog, fileName);
            
            DailyLogMemory.Instance.Reset();

            SetupTimer();
        }
        void CheckConfig()
        {
            if (fileHandlerConfig == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("PaymentTransactionsFileProcessorService does not have configuration");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
        public IFileProcessor<PaymentTransaction> AddConfiguration(IConfigurationSection configuration)
        {
            try
            {
                Console.WriteLine("Type 'start' to start the poolService");
                fileHandlerConfig = configuration.CheckHasValidConfig<FileProcessorConfig>();
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            return this;
        }
    }
}
