using System;
using System.Collections.Generic;
using System.IO;
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
            // Set properties from parameters
            AbsolutePath = absolutePath;
            AllTimestamps = allTimestamps;

            // Set other properties
            Name = Path.GetFileName(absolutePath);
            ParentDirAbsolutePath = Path.GetDirectoryName(absolutePath);
            ParentDirName = Path.GetFileName(ParentDirAbsolutePath);
            TimestampLastOpened = allTimestamps[0]; // TODO will this work in production? Must guarantee that the list of all timestamps is always ordered by timestamp descending
            NumViews = allTimestamps.Count;
        }
    }
}
