using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageArranger
{
    /// <summary>
    /// Associates an absolute path to a file with a timestamp.
    /// </summary>
    public class FileTimestampModel
    {
        // Properties
        public int Id { get; set; }
        public string FileAbsolutePath { get; set; }
        public string ParentDirAbsolutePath { get; set; }
        public long Ticks { get; set; }

        public DateTime TicksAsDateTime { get { return new DateTime(Ticks); } }


        // Constructors

        /// <summary>
        /// Required for how we query the database via SqliteDataAccess.GetAllTimestampsForFile()
        /// </summary>
        public FileTimestampModel() { }

        public FileTimestampModel(string absolutePath, long ticks)
        {
            // Set properties from parameters
            FileAbsolutePath = absolutePath;
            Ticks = ticks;

            // Set other parameters
            ParentDirAbsolutePath = Path.GetDirectoryName(absolutePath);
        }


        // Methods

        public override string ToString()
        {
            // TODO format timestamp as date string
            DateTime date = new DateTime(Ticks);
            return "" + date;
        }
    }
}
