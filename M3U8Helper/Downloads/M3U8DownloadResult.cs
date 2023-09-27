using System.Collections.Generic;
using M3U8Helper.Core;

namespace M3U8Helper.Downloads
{
    public class M3U8DownloadResult : OperateResult
    {
        #region Constructors

        public M3U8DownloadResult()
        {
            IsComplete = false;
            IsCancelled = false;
            DownloadedNodes = new List<M3U8Segment>();
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// 已下载的节点
        /// </summary>
        public List<M3U8Segment> DownloadedNodes { get; }

        /// <summary>
        /// 是否取消
        /// </summary>
        public bool IsCancelled { get; set; }

        /// <summary>
        /// 处理的节点
        /// </summary>
        public M3U8Segment LastNode { get; set; }

        #endregion Properties
    }
}