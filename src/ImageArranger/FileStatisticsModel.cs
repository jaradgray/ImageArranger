using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageArranger
{
    /// <summary>
    /// Represents a file item in the StatisticsWindow.
    /// </summary>
    public class FileStatisticsModel
    {
        public string Name { get; set; }
        public string AbsolutePath { get; set; }
        public string ParentDirAbsolutePath { get; set; }
        public FileTimestampModel TimestampLastOpened { get; set; }
        public List<FileTimestampModel> AllTimestamps { get; set; }

        public int NumViews
        {
            get;
            set;
        }

        public string ParentDirName
        {
            get;
        }


        // Constructor
        public FileStatisticsModel(string absolutePath, List<FileTimestampModel> allTimestamps)
        {
            AbsolutePath = absolutePath;
            AllTimestamps = allTimestamps;

            // TODO set the following correctly
            Name = "Name";
            ParentDirAbsolutePath = "ParentDirAbsolutePath";
            TimestampLastOpened = allTimestamps[0]; // TODO will this work in production? Must guarantee that the list of all timestamps is always ordered by timestamp descending
            NumViews = allTimestamps.Count;
            ParentDirName = "ParentDirName";
        }
    }
}
