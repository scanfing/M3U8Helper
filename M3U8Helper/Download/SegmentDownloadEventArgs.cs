using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuxiliaryTools.M3U8
{
    public class SegmentDownloadEventArgs : EventArgs
    {
        #region Constructors

        public SegmentDownloadEventArgs(M3U8Segment node, int percent = -1)
        {
            Segment = node;
            PercentComplete = percent;
        }

        #endregion Constructors

        #region Properties

        public bool IsSuccess { get; set; }
        public M3U8File M3U8File { get; set; }
        public int PercentComplete { get; }
        public M3U8Segment Segment { get; }

        #endregion Properties
    }
}