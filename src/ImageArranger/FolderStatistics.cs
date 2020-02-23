using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageArranger
{
    /// <summary>
    /// Represents a folder item in the StatisticsWindow.
    /// </summary>
    public class FolderStatistics
    {
        public string AbsolutePath { get; set; }

        public string Name
        {
            get;
        }

        public int NumViews
        {
            get;
            set;
        }
    }
}
