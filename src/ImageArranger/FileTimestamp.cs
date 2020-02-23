using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageArranger
{
    public class FileTimestamp
    {
        public string FileAbsolutePath { get; set; }
        public string DirAbsolutePath { get; set; }
        public long Millis { get; set; }
    }
}
