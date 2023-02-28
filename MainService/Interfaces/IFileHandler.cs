using MainService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainService.Interfaces
{
    public interface IFileHandler<TResponse>
    {
        //can be some factory 
        IParser<TResponse> Parser { get; set; }
        public FileHandleResponse Handle(string filePath, string destributionPath, string fileName);
    }
    
}
