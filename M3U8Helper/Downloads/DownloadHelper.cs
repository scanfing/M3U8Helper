using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using M3U8Helper.Core;

namespace M3U8Helper.Downloads
{
    public class DownloadHelper : Progress<M3U8VideoDownloadProgressChangedArgs>
    {
        #region Fields

        private WebClient _webClient;

        #endregion Fields

        #region Constructors

        public DownloadHelper()
        {
            _webClient = new WebClient();
        }

        #endregion Constructors

        #region Properties

        public int SegmentDownloadFailedRetryTimes { get; set; } = 3;

        #endregion Properties

        #region Methods

        /// <summary>
        /// 下载M3U8视频的头数据（Key 和 Map）（如果存在）
        /// </summary>
        /// <param name="head"></param>
        /// <param name="savedir"></param>
        public static bool DownloadM3U8Head(M3U8Head head, string savedir)
        {
            using (var client = new WebClient())
            {
                try
                {
                    if (head.IsEncrypt)
                    {
                        var kuri = new Uri(head.KeyUrl);
                        var kfile = kuri.LocalPath.Substring(kuri.LocalPath.LastIndexOf('/') + 1);
                        var keyfile = Path.Combine(savedir, kfile);
                        head.Key = client.DownloadData(head.KeyUrl);
                        File.WriteAllBytes(keyfile, head.Key);
                        head.KeyFile = keyfile;
                    }
                    if (head.HasMap)
                    {
                        var mapurl = new Uri(head.MapUrl);
                        var filename = mapurl.LocalPath.Substring(mapurl.LocalPath.LastIndexOf('/'));
                        var savefile = Path.Combine(savedir, filename);
                        client.DownloadFile(head.MapUrl, savefile);
                        head.MapFile = savefile;
                    }
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="node"></param>
        /// <param name="savedir"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="PathTooLongException"></exception>
        public static bool DownloadM3U8Segment(M3U8Segment node, string savedir, WebClient client = null)
        {
            try
            {
                System.Diagnostics.Trace.TraceInformation($"{DateTime.Now} {Thread.CurrentThread.ManagedThreadId} Download {node.SegmentName}");

                var file = Path.Combine(savedir, node.SegmentName);
                if (client == null)
                {
                    client = new WebClient();
                }

                client.DownloadFile(node.Target, file);
                if (File.Exists(file))
                {
                    var finfo = new FileInfo(file);
                    node.Size = finfo.Length;
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning($"Catch Ex When Download {node.SegmentName},{ex.Message}");
                return false;
            }
        }

        public static async Task DownloadM3U8SegmentsAsParallel(IEnumerable<M3U8Segment> segments, string savedir, CancellationToken token, bool skipexistfile = true, Action<M3U8Segment, bool> downloadaction = null, IEnumerable<KeyValuePair<string, string>> headers = null)
        {
            var lst = segments.ToList().OrderBy(p => p.SegmentName);
            var concurrentBag = new ConcurrentBag<M3U8Segment>(lst);
            var getitemaction = new Func<M3U8Segment>(() =>
             {
                 if (concurrentBag.TryTake(out var item))
                     return item;
                 return null;
             });
            var tlst = new List<Task>();
            for (var c = 0; c < 3; c++)
            {
                var client = new WebClient();
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        client.Headers.Add(header.Key, header.Value);
                    }
                }
                var t = Task.Factory.StartNew(() =>
                   {
                       while (concurrentBag.TryTake(out var node))
                       {
                           var succ = DownloadM3U8Segment(node, savedir, client);
                           downloadaction?.Invoke(node, succ);
                           if (token.IsCancellationRequested)
                               break;
                       }
                   }, token);
                tlst.Add(t);
            }
            await Task.WhenAll(tlst);
        }

        public async Task<M3U8DownloadResult> DownloadM3U8FilesAsParallel(M3U8File mfile, string savedir, CancellationToken token, bool skipexistfile = true, IEnumerable<KeyValuePair<string, string>> headers = null)
        {
            var result = new M3U8DownloadResult();
            try
            {
                Directory.CreateDirectory(savedir);
                DownloadM3U8Head(mfile.Head, savedir);
                if (token.IsCancellationRequested)
                {
                    result.IsCancelled = true;
                    return result;
                }
                var action = new Action<M3U8Segment, bool>((node, succ) =>
                  {
                      if (succ)
                          result.DownloadedNodes.Add(node);
                      var e = new M3U8VideoDownloadProgressChangedArgs(mfile, node)
                      {
                          IsNodeDownloadSuccess = succ,
                          Count = result.DownloadedNodes.Count
                      };
                      OnReport(e);
                  });
                await DownloadM3U8SegmentsAsParallel(mfile.Segments, savedir, token, skipexistfile, action, headers);
            }
            catch (Exception ex)
            {
                result.Exception = ex;
            }
            return result;
        }

        /// <summary>
        /// 异步下载M3U8视频
        /// </summary>
        /// <param name="target"></param>
        /// <param name="savedir"></param>
        public async Task<M3U8DownloadResult> DownloadM3U8VideoFilesAsync(M3U8File target, string savedir, CancellationToken token, bool skipexistsegment = false, IEnumerable<KeyValuePair<string, string>> headers = null)
        {
            var result = new M3U8DownloadResult();
            try
            {
                Directory.CreateDirectory(savedir);
                await Task.Factory.StartNew(() => DownloadM3U8Head(target.Head, savedir), token);
                var action = new Action<M3U8Segment, bool>((node, succ) =>
                {
                    if (succ)
                    {
                        result.DownloadedNodes.Add(node);
                    }
                    var arg = new M3U8VideoDownloadProgressChangedArgs(target, node)
                    {
                        IsNodeDownloadSuccess = succ,
                        Count = result.DownloadedNodes.Count
                    };
                    OnReport(arg);
                    token.ThrowIfCancellationRequested();
                });
                result.IsCancelled = await DownloadM3U8VideoSegments(target.Segments, savedir, skipexistsegment, action, headers);
                if (!result.IsCancelled)
                {
                    result.IsComplete = true;
                }
            }
            catch (Exception ex)
            {
                result.Exception = ex;
            }
            return result;
        }

        public async Task<bool> DownloadM3U8VideoSegments(M3U8Segment[] nodes, string savedir, bool skipexistfile = false, Action<M3U8Segment, bool> segmentdownloadedaction = null, IEnumerable<KeyValuePair<string, string>> headers = null)
        {
            var cancel = false;
            using (var client = new WebClient())
            {
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        client.Headers.Add(header.Key, header.Value);
                    }
                }
                foreach (var node in nodes)
                {
                    try
                    {
                        var succ = await Task.Factory.StartNew(() => DownloadM3U8Segment(node, savedir, client));
                        segmentdownloadedaction?.Invoke(node, succ);
                    }
                    catch (OperationCanceledException cex)
                    {
                        System.Diagnostics.Trace.TraceWarning($"{DateTime.Now} Download {node.SegmentName} Error,Ex:{cex.Message}");
                        segmentdownloadedaction?.Invoke(node, false);
                        cancel = true;
                        break;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.TraceWarning($"{DateTime.Now} Download {node.SegmentName} Error,Ex:{ex.Message}");
                        segmentdownloadedaction?.Invoke(node, false);
                    }
                }
            }
            return cancel;
        }

        #endregion Methods
    }
}