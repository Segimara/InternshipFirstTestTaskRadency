using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainService.Interfaces
{
    public interface IParser<T>
    {
        public T Parse(string line);
    }
}
