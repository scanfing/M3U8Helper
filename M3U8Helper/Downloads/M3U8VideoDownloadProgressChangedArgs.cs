using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using M3U8Helper.Core;

namespace M3U8Helper.Downloads
{
    public class M3U8VideoDownloadProgressChangedArgs : EventArgs
    {
        #region Constructors

        public M3U8VideoDownloadProgressChangedArgs(M3U8File file, M3U8Segment node)
        {
            File = file;
            Node = node;
            TotalCount = file.Segments.Length;
        }

        #endregion Constructors

        #region Properties

        public int Count { get; set; }
        public M3U8File File { get; private set; }

        public bool IsNodeDownloadSuccess { get; set; }

        public M3U8Segment Node { get; private set; }
        public int TotalCount { get; private set; }

        #endregion Properties
    }
}