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
    public class PaymentTransactionsFileProcessorService : IFileProcessor<PaymentTransaction>
    {
        private int maxThreads;
        bool isWorking = false;
        private DailyLogMemory DailyLogMemoryStore; //todo extract interface
        private FileProcessorConfig fileHandlerConfig;
        IFilePool _poolService;
        FileSystemWatcher watcher;
        System.Timers.Timer timer;
        IFileHandlerFactory<PaymentTransaction> _factory;
        private IFileHandler<PaymentTransaction> _fileHandler;
        private IDailyLogHandler<DailyLog> _dailyLogHandle;
        private object _lock = new object();

        public IFilePool TaskPool { get => _poolService; }

        public PaymentTransactionsFileProcessorService()
        {
            watcher = new FileSystemWatcher();
        }
        //add injection of IParserFactory 
        public PaymentTransactionsFileProcessorService(DailyLogMemory DailyLogMemoryStore,
            IFileHandlerFactory<PaymentTransaction> factory,
            IDailyLogHandler<DailyLog> dailyLogHandle,
            IFilePool filePool) : this()
        {
            this.DailyLogMemoryStore = DailyLogMemoryStore;
            this.fileHandlerConfig = fileHandlerConfig;
            _factory = factory;
            _dailyLogHandle = dailyLogHandle;
            _poolService = filePool;
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
        public PaymentTransactionsFileProcessorService SetupDailyStore(DailyLogMemory DailyLogMemoryStore)
        {
            this.DailyLogMemoryStore = DailyLogMemoryStore;
            return this;
        }
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
            _poolService.StartQueue();
            StartFileSystemWatcher();
            SetupTimer(fileHandlerConfig, DailyLogMemoryStore);
            Console.WriteLine("Application was started");
            AddExistingFiles();
        }
        public void Stop()
        {

            Console.WriteLine("Start pausing an application");
            _poolService.PauseProcessing();
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
                _poolService.AddAction(() => HandleFile(file));
            }
        }

        void HandleFile(string filePath)
        {
            lock (_lock)
            {
                var file = new FileInfo(filePath);
                if (_factory.Parsers.Keys.Any(x => x == "." + Path.GetExtension(filePath)))
                {
                    return;
                }
                var handler = _factory[file.Extension];
                //string subFolderPath = @"\" + DateTime.Now.ToString("MM-dd-yyyy");
                string subFolderPath = DateTime.Now.ToString("hh_mm");
                string subfolderCPath = Path.Combine(fileHandlerConfig.FolderB, subFolderPath);
                string fileDest = Path.Combine(subfolderCPath, "output" + UniqueFileNumber.Instance.GetUniqueNumber() + ".json");
                DailyLogMemoryStore.AddFileResponse(handler.Handle(filePath, fileDest));
            }
        }
        void StartFileSystemWatcher()
        {
            // Create a new FileSystemWatcher and set its properties.
            watcher.Path = fileHandlerConfig.FolderA;
            watcher.Filter = "*";
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.IncludeSubdirectories = false;

            // Add event handlers.
            watcher.Created += async (sender, e) =>
            {
                _poolService.AddAction(() => HandleFile(e.FullPath));
            };
            //watcher.Error += OnWatcherError;

            // Start monitoring.
            watcher.IncludeSubdirectories = false;
            watcher.EnableRaisingEvents = true;
        }
        void SetupTimer(FileProcessorConfig cfg, DailyLogMemory DailyLogMemoryStore)
        {
            DateTime now = DateTime.Now;
            //DateTime midnight = DateTime.Today.AddDays(1).AddTicks(-1);
            //TimeSpan timeUntilMidnight = midnight - now;
            //test 
            TimeSpan timeUntilMidnight = new TimeSpan(0, 0, 20);

            timer = new System.Timers.Timer(timeUntilMidnight.TotalMilliseconds);
            timer.AutoReset = false;
            timer.Elapsed += new ElapsedEventHandler((sender, e) =>
            {
                OnTimerElapsed(cfg, DailyLogMemoryStore);
            });
            timer.Enabled = true;
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
        private void OnTimerElapsed(FileProcessorConfig cfg, DailyLogMemory DailyLogMemoryStore)
        {
            if (DailyLogMemoryStore.GetParsedLines() == 0 && DailyLogMemoryStore.GetParsedFiles() == 0 && DailyLogMemoryStore.GetInvalidLines() == 0)
            {
                SetupTimer(cfg, DailyLogMemoryStore);
                return;
            }
            //string subFolderPath = @"\" + DateTime.Now.ToString("MM-dd-yyyy");
            string subFolderPath = DateTime.Now.ToString("hh_mm");
            string subfolderCPath = Path.Combine(cfg.FolderB, subFolderPath);

            // Create the subfolder C if it doesn't exist
            if (!Directory.Exists(subfolderCPath))
            {
                Directory.CreateDirectory(subfolderCPath);
            }
            string DailyLogPath = Path.Combine(subfolderCPath, "meta.log");
            // Get the parsed files count and parsed lines count from the dictionary
            var dailyLog = DailyLogMemoryStore.GetDailyLog();
            _dailyLogHandle.Handle(dailyLog, DailyLogPath);

            // Reset the dictionaries for the next day
            DailyLogMemoryStore.Reset();

            // Setup the timer for the next day
            SetupTimer(cfg, DailyLogMemoryStore);
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
