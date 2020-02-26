using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageArranger
{
    /// <summary>
    /// Represents a folder item in the StatisticsWindow.
    /// </summary>
    public class FolderStatisticsModel
    {
        // Properties

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

        /// <summary>
        /// The most recent timestamp in the database whose ParentDirAbsolutePath is this FolderStatisticsModel's AbsolutePath
        /// </summary>
        public FileTimestampModel MostRecentTimestamp
        {
            get;
            private set;
        }


        // Constructor

        public FolderStatisticsModel(string dirPath, List<FileTimestampModel> allTimestamps)
        {
            // Set properties from parameters
            AbsolutePath = dirPath;

            // Set other properties
            NumViews = allTimestamps.Count;
            Name = Path.GetFileName(dirPath);

            long mostTicks = 0;
            foreach (FileTimestampModel timestamp in allTimestamps)
            {
                if (timestamp.Ticks > mostTicks)
                {
                    MostRecentTimestamp = timestamp;
                    mostTicks = timestamp.Ticks;
                }
            }
        }
    }
}
