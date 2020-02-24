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
    public class FileTimestampModel
    {
        public int Id { get; set; }
        public string FileAbsolutePath { get; set; }
        public string ParentDirAbsolutePath { get; set; }
        public long Ticks { get; set; }


        // Methods

        public override string ToString()
        {
            // TODO format timestamp as date string
            DateTime date = new DateTime(Ticks);
            return "" + date;
        }
    }
}
