using MainService.Interfaces;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainService
{
    public class FileProcessorConfig 
    {
        public string FolderA { get; set; }
        public string FolderB { get; set; }
    }
}
