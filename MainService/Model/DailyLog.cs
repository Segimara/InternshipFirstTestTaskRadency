using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainService.Model
{
    public class DailyLog
    {
        public int ParsedFiles { get; set; }
        public int ParsedLines { get; set; }
        public int InvalidLines { get; set; }
        public IList<string> InvalidFileNames { get; set; }
    }
}
