using System;

namespace AuxiliaryTools.M3U8
{
    public class M3U8DownloadProgressChangedEventArgs : EventArgs
    {
        #region Constructors

        public M3U8DownloadProgressChangedEventArgs(M3U8File file)
        {
            M3U8File = file;
        }

        #endregion Constructors

        #region Properties

        public bool CancelDownload { get; set; } = false;
        public bool IsCancelled { get; set; }
        public bool IsComplete { get; set; }
        public bool IsInProgress { get; set; }
        public int LastIndex { get; set; }
        public M3U8File M3U8File { get; set; }

        #endregion Properties
    }
}