using System;
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

        #endregion Events

        #region Properties

        public int SegmentDownloadFailedRetryTimes { get; set; } = 3;

        #endregion Properties

        #region Methods

        /// <summary>
        /// </summary>
        /// <param name="node"></param>
        /// <param name="savedir"></param>
        /// <param name="skipexistfile"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        /// <exception cref="PathTooLongException"></exception>
        public static async Task DownloadM3U8SegmentTaskAsync(M3U8Segment node, string savedir, bool skipexistfile = false, WebClient client = null)
        {
            var file = Path.Combine(savedir, node.SegmentName);
            if (skipexistfile && File.Exists(file))
            {
                var finfo = new FileInfo(file);
                node.Size = finfo.Length;
                if (node.Size > 0)
                    return;
            }
            if (client == null)
            {
                client = new WebClient();
            }
            await client.DownloadFileTaskAsync(node.Target, file);
            if (File.Exists(file))
            {
                var finfo = new FileInfo(file);
                node.Size = finfo.Length;
            }
            return;
        }

        /// <summary>
        /// 下载M3U8视频的头数据（Key 和 Map）（如果存在）
        /// </summary>
        /// <param name="head"></param>
        /// <param name="savedir"></param>
        public void DownloadM3U8Head(M3U8Head head, string savedir)
        {
            using (var client = new WebClient())
            {
                if (head.IsEncrypt)
                {
                    var kuri = new Uri(head.KeyUrl);
                    var kfile = kuri.LocalPath.Substring(kuri.LocalPath.LastIndexOf('/') + 1);
                    var keyfile = Path.Combine(savedir, kfile);
                    head.Key = _webClient.DownloadData(head.KeyUrl);
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
            }
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
                await Task.Factory.StartNew(() => DownloadM3U8Head(target.Head, savedir), token);
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
            var segresult = await DownloadM3U8VideoSegments(target.Segments, savedir, 0, -1, skipexistsegment, action);
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

        public async Task<M3U8DownloadResult> DownloadM3U8VideoSegments(M3U8Segment[] nodes, string savedir, int offset = 0, int length = -1, bool skipexistfile = false, Action<M3U8Segment> progresschangedaction = null)
        {
            using (var client = new WebClient())
            {
                var result = new M3U8DownloadResult();
                var endvalue = nodes.Length;
                if (length >= 0)
                {
                    endvalue = Math.Min(endvalue, offset + length);
                }
                for (var index = offset; index < endvalue; index++)
                {
                    try
                    {
                        result.LastNode = nodes[index];
                        await DownloadM3U8SegmentTaskAsync(result.LastNode, savedir, skipexistfile, client);
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

        #endregion Methods
    }
}