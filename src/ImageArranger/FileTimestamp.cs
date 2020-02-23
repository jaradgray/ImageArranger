using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageArranger
{
    /// <summary>
    /// Associates an absolute path to a file with a timestamp.
    /// </summary>
    public class FileTimestamp
    {
        public string FileAbsolutePath { get; set; }
        public string DirAbsolutePath { get; set; }
        public long Millis { get; set; }
    }
}
