using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainService.Interfaces
{
    public interface IFileProcessor
    {
        IFilePool FilePool { get; set; }
        void Start();
        void Stop();
        void Reset();
        void Run();
        IFileProcessor AddConfiguration(IConfiguration configuration);
    }
}
