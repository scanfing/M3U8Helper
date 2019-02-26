using System;

namespace AuxiliaryTools.M3U8
{
    public class M3U8SegmentDownloadedEventArgs : EventArgs
    {
        #region Constructors

        public M3U8SegmentDownloadedEventArgs(M3U8Segment node, M3U8File file)
        {
            Segment = node;
            M3U8File = file;
        }

        #endregion Constructors

        #region Properties

        public int DownloadedCount { get; set; }

        public bool IsCancelled { get; set; }

        public bool IsComplete { get; set; }

        public bool IsInProgress { get; set; }

        public M3U8File M3U8File { get; set; }

        public M3U8Segment Segment { get; }

        public int TotalCount { get; set; }

        #endregion Properties
    }
}