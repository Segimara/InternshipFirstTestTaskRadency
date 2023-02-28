using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainService.Interfaces
{
    public interface IFileHandlerFactory<T> 
    {
        IDictionary<string, IFileHandler<T>> Parsers { get; }
        IFileHandler<T> this[string key] { get; }
    }
}
