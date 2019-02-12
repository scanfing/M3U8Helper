using System;

namespace AuxiliaryTools.M3U8
{
    public class OperateResult
    {
        #region Properties

        /// <summary>
        /// 引发失败的异常
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// 是否完成
        /// </summary>
        public bool IsComplete { get; set; }

        public string Message { get; set; }

        #endregion Properties
    }
}