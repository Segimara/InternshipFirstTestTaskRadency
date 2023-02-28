using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainService.Model
{
    public class FileHandleResponse
    {
        public bool isInvalid { get; set; }
        public int ParsedLines { get; set; }
        public int InvalidLines { get; set; }
        public string FileName { get; set; }
        public object Data { get; set; }
    }
}
