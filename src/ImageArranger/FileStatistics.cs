using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageArranger
{
    public class FileStatistics
    {
        public string Name { get; set; }
        public string AbsolutePath { get; set; }
        public string ParentDirAbsolutePath { get; set; }
        public FileTimestamp TimestampLastOpened { get; set; }

        public int NumViews
        {
            get;
            set;
        }

        public string ParentDirName
        {
            get;
        }
    }
}
