using System;

namespace AuxiliaryTools.M3U8
{
    public class M3U8Segment
    {
        #region Constructors

        public M3U8Segment(Uri targeturl, double secs)
        {
            Target = targeturl;
            Seconds = secs;
            SegmentName = targeturl.LocalPath.Substring(targeturl.LocalPath.LastIndexOf('/') + 1);
            Size = 0;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// 片段是否单独加密
        /// </summary>
        public bool IsEncrypt { get; set; }

        /// <summary>
        /// 解密片段的Key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 解密片段的Key所在的Url地址
        /// </summary>
        public string KeyUrl { get; set; }

        /// <summary>
        /// 片段时长（秒）
        /// </summary>
        public double Seconds { get; private set; }

        /// <summary>
        /// 片段名 纯名字
        /// </summary>
        public string SegmentName { get; private set; }

        /// <summary>
        /// 片段大小（字节）
        /// </summary>
        public long Size { get; set; }

        /// <summary>
        /// 目标地址
        /// </summary>
        public Uri Target { get; private set; }

        #endregion Properties

        #region Methods

        public override string ToString()
        {
            if (Size <= 0)
                return $"{SegmentName}, {Seconds}s";
            else
                return $"{SegmentName}, {Seconds}s, {Size / 1024}KB";
        }

        #endregion Methods
    }
}
