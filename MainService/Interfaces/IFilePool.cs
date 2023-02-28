using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainService.Interfaces
{
    public interface IFilePool
    {
        public bool isStarted { get; set; } 
        public void AddAction(Action action);
        public void StartQueue();
        public void PauseProcessing();
        public void CleanQueue();
    }
}
