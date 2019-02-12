using System;

namespace AuxiliaryTools.M3U8
{
    public class M3U8SegmentsCombineProgressChangedEventArgs : EventArgs
    {
        #region Properties

        public int CurrIndex { get; set; }
        public bool CurrNodeCombined { get; set; }
        public bool IsCancelled { get; set; }
        public bool IsComplete { get; set; }
        public bool IsInProgress { get; set; }
        public M3U8File M3U8File { get; set; }

        #endregion Properties
    }
}