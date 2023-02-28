using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainService.Services
{
    public class UniqueFileNumber
    {
        volatile int _number;
        private readonly object _lock = new object();
        public UniqueFileNumber()
        {
            _number = 0;
        }
        public int GetUniqueNumber()
        {
            lock (_lock)
            {
                return _number++;
            }
        }
        private static UniqueFileNumber _instance;
        public static UniqueFileNumber Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new UniqueFileNumber();
                }
                return _instance;
            }
        }
    }
}
