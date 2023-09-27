using M3U8Helper.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace M3U8Helper
{
    public class M3U8Helper
    {
        #region Methods

        /// <summary>
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static byte[] AesDecrypt(byte[] data, byte[] key, string iv = "")
        {
            using (var rm = new RijndaelManaged())
            {
                if (key == null || key.Length == 0)
                    return data;
                rm.Key = key;
                if (!string.IsNullOrWhiteSpace(iv))
                {
                    var tmpiv = iv.Replace("0x", "").Trim();
                    var buff = new byte[tmpiv.Length / 2];
                    for (var index = 0; index < buff.Length; index++)
                    {
                        var vstr = tmpiv.Substring(index * 2, 2);
                        buff[index] = Convert.ToByte(vstr, 16);
                    }
                    rm.IV = buff;
                }
                ICryptoTransform cTransform = rm.CreateDecryptor();
                var resultArray = cTransform.TransformFinalBlock(data, 0, data.Length);
                return resultArray;
            }
        }

        /// <summary>
        /// 解密（如果需要）并合并M3U8片段为一个文件
        /// </summary>
        /// <param name="target"></param>
        /// <param name="srcdir"></param>
        /// <param name="dstfile"></param>
        /// <param name="ignoreerror"></param>
        public static async Task<OperateResult> CombineM3U8Segments(M3U8File target, string srcdir, string dstfile, Action<M3U8SegmentsCombineProgressChangedEventArgs> progressChangedAction = null, bool ignoreerror = true)
        {
            var result = new OperateResult();
            try
            {
                using (var fs = new FileStream(dstfile, FileMode.Create, FileAccess.ReadWrite))
                {
                    var totallength = target.Segments.Length;
                    for (var index = 0; index < totallength; index++)
                    {
                        var node = target.Segments[index];
                        var srcfile = Path.Combine(srcdir, node.SegmentName);
                        var arg = new M3U8SegmentsCombineProgressChangedEventArgs
                        {
                            M3U8File = target,
                            IsInProgress = true,
                            CurrIndex = index,
                            IsComplete = false
                        };
                        if (File.Exists(srcfile))
                        {
                            if (target.Head.IsEncrypt || node.IsEncrypt)
                            {
                                var buff = File.ReadAllBytes(srcfile);
                                var data = AesDecrypt(buff, target.Head.Key, target.Head.IV);
                                await fs.WriteAsync(data, 0, data.Length);
                            }
                            else
                            {
                                var srcfs = new FileStream(srcfile, FileMode.Open);
                                await srcfs.CopyToAsync(fs);
                                fs.Flush();
                            }
                            arg.CurrNodeCombined = true;
                        }
                        else
                        {
                            if (ignoreerror)
                                arg.CurrNodeCombined = false;
                            else
                                throw new FileNotFoundException();
                        }
                        progressChangedAction?.Invoke(arg);
                    }
                }
                result.IsComplete = true;
            }
            catch (Exception ex)
            {
                result.Exception = ex;
                System.Diagnostics.Trace.TraceWarning($"CombineM3U8Segments Ex:{ex.Message}");
            }
            return result;
        }

        /// <summary>
        /// 下载M3U8列表文件（非源视频文件）
        /// </summary>
        /// <param name="url"></param>
        /// <param name="localdir"></param>
        /// <returns></returns>
        public static string[] DownloadM3U8ListFile(string url, string localdir)
        {
            var dir = localdir.TrimEnd('\\') + "\\";
            Directory.CreateDirectory(dir);
            var ms = M3U8FileAnalyzer.AnalyzeM3U8Url(url);
            var lst = new List<string>();
            foreach (var m in ms)
            {
                var name = Path.GetFileNameWithoutExtension(m.SourceUrl.LocalPath);
                var mfile = $"{dir}{name}.m3u8";
                if (File.Exists(mfile))
                {
                    mfile = $"{dir}{name}_{m.RESOLUTION}.m3u8";
                }
                try
                {
                    SaveToFile(m, mfile);
                    lst.Add(mfile);
                }
                catch
                {
                }
            }
            return lst.ToArray();
        }

        public static string GetM3U8Content(M3U8File m3u8, bool keepurl)
        {
            StringBuilder sb = new StringBuilder();
            var maxd = m3u8.Segments.Max(p => p.Seconds);
            maxd = Math.Truncate(maxd) + 1;
            sb.Append("#EXTM3U\n");
            sb.Append($"#EXT-X-VERSION:{m3u8.Head.VERSION}\n");
            sb.Append($"#EXT-X-TARGETDURATION:{m3u8.Head.TARGETDURATION}\n");
            sb.Append($"#EXT-X-MEDIA-SEQUENCE:{m3u8.Head.MEDIA_SEQUENCE}\n");
            if (keepurl)
            {
                if (m3u8.Head.HasMap)
                {
                    sb.Append($"#EXT-X-MAP:URI=\"{m3u8.Head.MapUrl}\",\n");
                }
                if (m3u8.Head.IsEncrypt)
                {
                    sb.Append($"#EXT-X-KEY:METHOD={m3u8.Head.EncryptMethod},KEYURI=\"{m3u8.Head.KeyUrl}\"\n");
                }
                foreach (var node in m3u8.Segments)
                {
                    sb.Append($"#EXTINF:{node.Seconds},\n");
                    sb.Append($"{node.Target}\n");
                }
            }
            else
            {
                if (m3u8.Head.HasMap)
                {
                    var mapurl = new Uri(m3u8.Head.MapUrl);
                    sb.Append($"#EXT-X-MAP:URI=\"{mapurl.LocalPath.Substring(mapurl.LocalPath.LastIndexOf('/') + 1)}\",\n");
                }
                if (m3u8.Head.IsEncrypt)
                {
                    var keyurl = new Uri(m3u8.Head.KeyUrl);
                    sb.Append($"#EXT-X-KEY:METHOD={m3u8.Head.EncryptMethod},KEYURI=\"{keyurl.LocalPath.Substring(keyurl.LocalPath.LastIndexOf('/') + 1)}\"\n");
                }
                foreach (var node in m3u8.Segments)
                {
                    sb.Append($"#EXTINF:{node.Seconds},\n");
                    sb.Append($"{node.SegmentName}\n");
                }
            }
            sb.Append("#EXT-X-ENDLIST\n");
            return sb.ToString();
        }

        public static void SaveToFile(M3U8File m3u8, string filename, bool keepurl = true)
        {
            var content = GetM3U8Content(m3u8, keepurl);
            File.WriteAllText(filename, content);
        }

        #endregion Methods
    }
}