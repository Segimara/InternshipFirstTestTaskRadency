﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainService.Interfaces
{
    public interface IDailyLogHandler<T> 
    {
        void Handle(T dailyLog, string fileName);
    }
}
