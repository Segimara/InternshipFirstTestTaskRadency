using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainService.Interfaces
{
    public interface IParserFactory
    {
        IDictionary<string, Type> Parsers { get; }
        Type this[string key] { get; }
    }
}
