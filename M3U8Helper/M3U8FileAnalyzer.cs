using M3U8Helper.Core;
using System;
using System.Collections.Generic;
using System.Net;

namespace M3U8Helper
{
    public class M3U8FileAnalyzer
    {
        #region Methods

        public static M3U8File AnalyzeM3U8Content(Uri url, string content)
        {
            if (!content.StartsWith("#EXTM3U"))
                return null;
            var path = url.LocalPath;
            path = path.Substring(0, path.LastIndexOf('/') + 1);
            var fixurl = url.Scheme + "://" + url.Authority + path;
            var fixuri = new Uri(fixurl);
            var head = AnalyzeM3U8Head(fixuri, content);
            var nodes = AnalyzeSegments(fixuri, content);
            var m3u8 = new M3U8File(url, head, nodes);
            return m3u8;
        }

        public static M3U8Head AnalyzeM3U8Head(Uri urlfix, string content)
        {
            if (content.StartsWith("#EXTM3U"))
            {
                var head = new M3U8Head();
                var strs = content.Split('\n');
                foreach (var str in strs)
                {
                    if (str.StartsWith("#EXTINF:"))
                        break;
                    if (str.StartsWith("#EXT-X-VERSION:"))
                    {
                        head.VERSION = str.Substring(15);
                    }
                    else if (str.StartsWith("#EXT-X-TARGETDURATION:"))
                    {
                        var durstr = str.Substring(22);
                        if (double.TryParse(durstr, out var dr))
                            head.TARGETDURATION = dr;
                    }
                    else if (str.StartsWith("#EXT-X-MEDIA-SEQUENCE:"))
                    {
                        var seqstr = str.Substring(22);
                        if (int.TryParse(seqstr, out var seq))
                            head.MEDIA_SEQUENCE = seq;
                    }
                    else if (str.StartsWith("#EXT-X-MAP:"))
                    {
                        head.HasMap = true;
                        var strr = str.Substring(11);
                        var init = strr.Substring(4);//Trim URI=
                        var map = init.Trim('\"');
                        if (map.Contains("://"))
                        {
                            head.MapUrl = map;
                        }
                        else if (map.StartsWith("/"))
                        {
                            head.MapUrl = urlfix.Scheme + "://" + urlfix.Authority + map;
                        }
                        else
                        {
                            head.MapUrl = urlfix + map;
                        }
                    }
                    else if (str.StartsWith("#EXT-X-KEY:"))
                    {
                        var strr = str.Substring(11);
                        var kvs = strr.Split(',');
                        foreach (var nvs in kvs)
                        {
                            var nv = nvs.Split('=');
                            if (nv[0] == "METHOD")
                            {
                                head.IsEncrypt = true;
                                head.EncryptMethod = nv[1];
                            }
                            else if (nv[0] == "URI")
                            {
                                var k = nv[1].Trim('\"');
                                if (k.Contains(":"))
                                    head.KeyUrl = k;
                                else if (k.StartsWith("/"))
                                {
                                    head.KeyUrl = urlfix.Scheme + "://" + urlfix.Authority + k;
                                }
                                else
                                {
                                    head.KeyUrl = urlfix + k;
                                }
                            }
                            else if (nv[0] == "IV")
                            {
                                head.IV = nv[1];
                            }
                        }
                    }
                }
                return head;
            }
            return null;
        }

        public static M3U8File[] AnalyzeM3U8Url(string url, IEnumerable<KeyValuePair<string, string>> headers = null)
        {
            if (url.EndsWith("/"))
            {
                url = url.TrimEnd('/');
            }
            var uri = new Uri(url);
            WebClient wclient = new WebClient();
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    wclient.Headers.Add(header.Key, header.Value);
                }
            }
            var content = wclient.DownloadString(uri);
            if (content.Contains(".m3u8"))
            {
                return AnalyzeTopM3U8(url, content);
            }
            else
            {
                var m3u8 = AnalyzeM3U8Content(uri, content);
                return new M3U8File[] { m3u8 };
            }
        }

        public static M3U8Segment[] AnalyzeSegments(Uri urlfix, string content)
        {
            var lst = new List<M3U8Segment>();
            var strs = content.Split('\n');
            for (var index = 0; index < strs.Length; index++)
            {
                var str = strs[index].Trim();
                if (str.StartsWith("#EXTINF:"))
                {
                    str = str.Remove(0, 8).TrimEnd(',');
                    if (double.TryParse(str, out var dr))
                    {
                        var target = strs[++index];
                        if (target.StartsWith("/"))
                        {
                            target = urlfix.Scheme + "://" + urlfix.Authority + target;
                        }
                        else if (!target.Contains("://"))
                        {
                            target = urlfix + target;
                        }
                        var segment = new M3U8Segment(new Uri(target), dr);
                        lst.Add(segment);
                    }
                }
            }
            return lst.ToArray();
        }

        public static M3U8File[] AnalyzeTopM3U8(string srcurl, string content)
        {
            var uri = new Uri(srcurl);
            var strs = content.Split('\n');
            var lst = new List<M3U8File>();
            var srcpath = uri.LocalPath;
            srcpath = srcpath.Substring(0, srcpath.LastIndexOf('/') + 1);
            for (var index = 0; index < strs.Length; index++)
            {
                var str = strs[index];
                if (str.StartsWith("#EXT-X-STREAM-INF:"))
                {
                    var bdwidth = -1;
                    var rostr = "";
                    if (str.ToUpper().Contains("BANDWIDTH="))
                    {
                        var bdstr = str.Substring(str.ToUpper().IndexOf("BANDWIDTH=") + 10);
                        bdstr = bdstr.Split(',')[0];
                        if (int.TryParse(bdstr, out var v))
                        {
                            bdwidth = v;
                        }
                    }
                    if (str.ToUpper().Contains("RESOLUTION="))
                    {
                        rostr = str.Substring(str.ToUpper().IndexOf("RESOLUTION=") + 11);
                        rostr = rostr.Split(',')[0];
                    }
                    var data = strs[++index];
                    if (!data.Contains("://"))
                    {
                        if (data.StartsWith("/"))
                            data = uri.Scheme + "://" + uri.Host + data;
                        else
                            data = uri.Scheme + "://" + uri.Authority + srcpath + data;
                        var ms = AnalyzeM3U8Url(data);
                        foreach (var m in ms)
                        {
                            m.BANDWIDTH = bdwidth;
                            m.RESOLUTION = rostr;
                        }
                        lst.AddRange(ms);
                    }
                }
            }
            return lst.ToArray();
        }

        #endregion Methods
    }
}