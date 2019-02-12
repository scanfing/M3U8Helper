using System;
using System.Text;

namespace M3U8Downloader.Infrastruction
{
    public static class Extensition
    {
        #region Methods

        /// <summary>
        /// 获取十进制值的字符串表现形式
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static string ToDecString(this byte[] src)
        {
            return src?.ToString(10);
        }

        /// <summary>
        /// 获取十六进制值的字符串表现形式
        /// </summary>
        /// <param name="src"></param>
        /// <returns></returns>
        public static string ToHexString(this byte[] src)
        {
            return src?.ToString(16);
        }

        /// <summary>
        /// 转换为 *进制的字符串表现形式
        /// </summary>
        /// <param name="src"></param>
        /// <param name="tobase">目标进制值</param>
        /// <returns></returns>
        public static string ToString(this byte[] src, int tobase)
        {
            if (src == null)
            {
                return null;
            }
            if (src.Length == 0)
            {
                return string.Empty;
            }
            StringBuilder sb = new StringBuilder();
            foreach (byte b in src)
            {
                var v = Convert.ToString(b, tobase);
                sb.AppendFormat("{0} ", v.PadLeft(2, '0'));
            }
            if (sb.Length > 1)
            {
                sb.Remove(sb.Length - 1, 1);
            }
            return sb.ToString();
        }

        #endregion Methods
    }
}