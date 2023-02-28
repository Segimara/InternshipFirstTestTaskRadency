using MainService.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MainService.Services
{
    //need to implement Dispose pattern
    public class FileParallelPoolService : IFilePool, IDisposable
    {
        private readonly int _batchSize = 4;    
        public bool isStarted { get; set; } = false;
        List<Task> tasks = new List<Task>();
        private readonly ConcurrentQueue<Action> fileQueue = new ConcurrentQueue<Action>();
        private readonly CancellationTokenSource processingCancellationTokenSource = new CancellationTokenSource();

        public FileParallelPoolService(int NumberOfProcessor)
        {
            _batchSize = NumberOfProcessor;
        }
        private void ProcessFileBatch(CancellationToken cancellationToken)
        {
            while (fileQueue.TryDequeue(out Action process) && !cancellationToken.IsCancellationRequested)
            {
                var task = Task.Run(() =>
                {
                    process();
                    GC.Collect();
                }, cancellationToken);

                tasks.Add(task);

                if (tasks.Count >= Environment.ProcessorCount)
                {
                    Task.WaitAll(tasks.ToArray(), cancellationToken);
                }
            }

            Task.WaitAll(tasks.ToArray(), cancellationToken);
            
        }

        public void AddAction(Action action)
        {
            fileQueue.Enqueue(action);

            if (fileQueue.Count <= _batchSize && isStarted)
            {
                ProcessFileBatch(processingCancellationTokenSource.Token);
            }
        }
        public void StartQueue()
        {
            isStarted = true;
            if (fileQueue.Count > 0)
            {
                
                ProcessFileBatch(processingCancellationTokenSource.Token);
            }
        }
        public void PauseProcessing()
        {
            Task.WaitAll(tasks.ToArray());
            isStarted = false;
        }
        public void CleanQueue()
        {
            while (fileQueue.TryDequeue(out Action _)) { }
        }

    public void Dispose()
    {
        processingCancellationTokenSource.Cancel();
            
        Task.WaitAll(tasks.ToArray());
            
        processingCancellationTokenSource.Dispose();
    }
    }
}
