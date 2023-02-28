using MainService.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Timers;
using MainService.Extension;

namespace MainService.Services
{
    public class FileProcessorService : IFileProcessor
    {
        private int maxThreads;
        private DailyLogMemory DailyLogMemoryStore; //todo extract interface
        private FileProcessorConfig fileHandlerConfig;

        public IFilePool FilePool { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public FileProcessorService()
        {

        }
        //add injection of IParserFactory 
        public FileProcessorService(DailyLogMemory DailyLogMemoryStore, FileProcessorConfig fileHandlerConfig)
        {
            this.DailyLogMemoryStore = DailyLogMemoryStore;
            this.fileHandlerConfig = fileHandlerConfig;
        }
        public void Run()
        {
            string command = "start";
            bool work = true;
            while (work)
            {
                switch (command)
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
                            break;
                        }
                    case "exit":
                        {
                            ForceStop();
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
            throw new NotImplementedException();
        }

        public FileProcessorService AddConfiguration(FileProcessorConfig config)
        {
            this.fileHandlerConfig = config;
            return this;
        }
        public FileProcessorService AddDailyStore(FileProcessorConfig config)
        {
            this.fileHandlerConfig = config;
            return this;
        }
        public void Start()
        {
            Console.WriteLine("Application was started");
            CheckConfig();
            InitFolders(fileHandlerConfig);
            FileParalelPoolService service = new FileParalelPoolService(50, DailyLogMemoryStore);
            AddExistingFiles(fileHandlerConfig, service);
            StartFileSystemWatcher(service, fileHandlerConfig);
            SetupTimer(fileHandlerConfig, DailyLogMemoryStore);
        }
        public void Stop()
        {

        }
        public void Reset()
        {

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
        void AddExistingFiles(FileProcessorConfig config, FileParalelPoolService service)
        {
            string[] files = Directory.GetFiles(Path.GetFullPath(config.FolderA), "*.txt");
            foreach (var file in files)
            {
                service.AddFile(file, config.FolderB);
            }
        }
        void StartFileSystemWatcher(FileParalelPoolService service, FileProcessorConfig config)
        {
            // Create a new FileSystemWatcher and set its properties.
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = config.FolderA;
            watcher.Filter = "*.txt";
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.IncludeSubdirectories = false;

            // Add event handlers.
            watcher.Created += (sender, e) =>
            {
                service.AddFile(e.FullPath, config.FolderB);
            };
            watcher.Changed += (sender, e) =>
            {
                service.AddFile(e.FullPath, config.FolderB);
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

            var timer = new System.Timers.Timer(timeUntilMidnight.TotalMilliseconds);
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
                Console.WriteLine("FileProcessorService does not have configuration");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
        private void OnTimerElapsed(FileProcessorConfig cfg, DailyLogMemory DailyLogMemoryStore)
        {
            if (DailyLogMemoryStore.GetParsedLines() == 0 && DailyLogMemoryStore.GetParsedFiles() == 0 && DailyLogMemoryStore.GetErrorsLines() == 0)
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
            string DailyLogPath = Path.Combine(subfolderCPath, "Daily.log");
            // Get the parsed files count and parsed lines count from the dictionary
            int parsedFilesCount = DailyLogMemoryStore.GetParsedFiles();
            int parsedLinesCount = DailyLogMemoryStore.GetParsedLines();

            // Get the invalid files count and paths from the dictionary
            int invalidFilesCount = DailyLogMemoryStore.GetInvalidFilesCount();
            IList<string> invalidFilePaths = DailyLogMemoryStore.GetInvalidFiles();

            using (StreamWriter writer = File.CreateText(DailyLogPath))
            {
                writer.WriteLine($"parsed_files: {parsedFilesCount}");
                writer.WriteLine($"parsed_lines: {parsedLinesCount}");
                writer.WriteLine($"found_errors: {invalidFilesCount}");
                writer.WriteLine($"invalid_files: [{string.Join(", ", invalidFilePaths)}]");
            }

            // Reset the dictionaries for the next day
            DailyLogMemoryStore.Reset();

            // Setup the timer for the next day
            SetupTimer(cfg, DailyLogMemoryStore);
        }
        public IFileProcessor AddConfiguration(IConfiguration configuration)
        {
            try
            {
                Console.WriteLine("Type 'start' to start the service");
                fileHandlerConfig = configuration.CheckHasValidConfig<FileProcessorConfig>();
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e.Message);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
    }
}
