using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace AuxiliaryTools.M3U8
{
    public class DownloadHelper
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

        #region Events

        public event EventHandler<M3U8DownloadProgressChangedEventArgs> DownloadProgressChanged;

        public event EventHandler<SegmentDownloadEventArgs> SegmentDownloaded;

        #endregion Events

        #region Properties

        public int SegmentDownloadFailedRetryTimes { get; set; } = 3;

        #endregion Properties

        #region Methods

        /// <summary>
        /// 下载M3U8视频的头数据（Key 和 Map）（如果存在）
        /// </summary>
        /// <param name="head"></param>
        /// <param name="savedir"></param>
        public static async Task<bool> DownloadM3U8HeadTaskAsync(M3U8Head head, string savedir)
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
                        head.Key = await client.DownloadDataTaskAsync(head.KeyUrl);
                        File.WriteAllBytes(keyfile, head.Key);
                        head.KeyFile = keyfile;
                    }
                    if (head.HasMap)
                    {
                        var mapurl = new Uri(head.MapUrl);
                        var filename = mapurl.LocalPath.Substring(mapurl.LocalPath.LastIndexOf('/'));
                        var savefile = Path.Combine(savedir, filename);
                        await client.DownloadFileTaskAsync(head.MapUrl, savefile);
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

        public static async Task DownloadM3U8SegmentsAsParallel(IEnumerable<M3U8Segment> segments, string savedir, CancellationToken token, bool skipexistfile = true, Action<M3U8Segment, int> downloadaction = null)
        {
            var lst = segments.Reverse();
            var concurrentBag = new ConcurrentBag<M3U8Segment>(lst);
            var getitemaction = new Func<M3U8Segment>(() =>
             {
                 if (concurrentBag.TryTake(out var item))
                     return item;
                 return null;
             });
            var tlst = new List<Task>();
            var total = segments.Count();
            var index = 0;
            while (concurrentBag.TryTake(out var node))
            {
                var t = Task.Run(async () =>
                {
                    await DownloadM3U8SegmentTaskAsync(node, savedir);
                    var curindex = Interlocked.Increment(ref index);
                    var percent = curindex * 100 / total;
                    downloadaction?.Invoke(node, percent);
                }, token);
                tlst.Add(t);
                await Task.Delay(10);
            }
            await Task.WhenAll(tlst);
        }

        /// <summary>
        /// </summary>
        /// <param name="node"></param>
        /// <param name="savedir"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="PathTooLongException"></exception>
        public static async Task<bool> DownloadM3U8SegmentTaskAsync(M3U8Segment node, string savedir, WebClient client = null)
        {
            try
            {
                var file = Path.Combine(savedir, node.SegmentName);
                if (client == null)
                {
                    client = new WebClient();
                }
                await client.DownloadFileTaskAsync(node.Target, file);
                if (File.Exists(file))
                {
                    var finfo = new FileInfo(file);
                    node.Size = finfo.Length;
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task DownloadM3U8FilesAsParallel(M3U8File mfile, string savedir, CancellationToken token, bool skipexistfile = true)
        {
            await DownloadM3U8HeadTaskAsync(mfile.Head, savedir);
            if (token.IsCancellationRequested)
                return;
            var downloadaction = new Action<M3U8Segment, int>((node, percentcomplte) =>
             {
                 var e = new SegmentDownloadEventArgs(node, percentcomplte)
                 {
                     M3U8File = mfile
                 };
                 RaiseSegmentDownloaded(e);
             });
            await DownloadM3U8SegmentsAsParallel(mfile.Segments, savedir, token, skipexistfile, downloadaction);
        }

        /// <summary>
        /// 异步下载M3U8视频
        /// </summary>
        /// <param name="target"></param>
        /// <param name="savedir"></param>
        public async Task<M3U8DownloadResult> DownloadM3U8VideoFilesAsync(M3U8File target, string savedir, CancellationToken token, bool skipexistsegment = false)
        {
            try
            {
                Directory.CreateDirectory(savedir);
                await Task.Factory.StartNew(() => DownloadM3U8HeadTaskAsync(target.Head, savedir), token);
            }
            catch (Exception ex)
            {
                var rt = new M3U8DownloadResult()
                {
                    Exception = ex
                };
                return rt;
            }
            var firarg = new M3U8DownloadProgressChangedEventArgs(target)
            {
                IsInProgress = true,
                LastIndex = -1
            };
            RaiseProgressChanged(firarg);
            var action = new Action<M3U8Segment>((node) =>
            {
                var arg = new M3U8DownloadProgressChangedEventArgs(target)
                {
                    IsInProgress = true,
                    LastIndex = target.Segments.ToList().IndexOf(node)
                };
                RaiseProgressChanged(arg);
                if (arg.CancelDownload)
                    throw new OperationCanceledException("下载被取消");
                token.ThrowIfCancellationRequested();
            });
            var segresult = await DownloadM3U8VideoSegments(target.Segments, savedir, skipexistsegment, action);
            var endargs = new M3U8DownloadProgressChangedEventArgs(target)
            {
                IsInProgress = false,
                IsComplete = segresult.IsComplete,
                IsCancelled = segresult.IsCancelled,
                LastIndex = target.Segments.ToList().IndexOf(segresult.LastNode)
            };
            RaiseProgressChanged(endargs);
            return segresult;
        }

        public async Task<M3U8DownloadResult> DownloadM3U8VideoSegments(M3U8Segment[] nodes, string savedir, bool skipexistfile = false, Action<M3U8Segment> progresschangedaction = null)
        {
            using (var client = new WebClient())
            {
                var result = new M3U8DownloadResult();
                var endvalue = nodes.Length;
                for (var index = 0; index < endvalue; index++)
                {
                    try
                    {
                        result.LastNode = nodes[index];
                        await DownloadM3U8SegmentTaskAsync(result.LastNode, savedir, client);
                        result.DownloadedNodes.Add(result.LastNode);
                        progresschangedaction?.Invoke(result.LastNode);
                    }
                    catch (OperationCanceledException cex)
                    {
                        result.IsCancelled = true;
                        result.Exception = cex;
                        return result;
                    }
                    catch (Exception ex)
                    {
                        result.Exception = ex;
                        return result;
                    }
                }
                result.IsComplete = true;
                return result;
            }
        }

        private void RaiseProgressChanged(M3U8DownloadProgressChangedEventArgs args)
        {
            DownloadProgressChanged?.Invoke(this, args);
        }

        private void RaiseSegmentDownloaded(SegmentDownloadEventArgs args)
        {
            SegmentDownloaded?.Invoke(this, args);
        }

        #endregion Methods
    }
}