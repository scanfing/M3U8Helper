using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AuxiliaryTools.M3U8;
using M3U8Downloader.Infrastruction;
using M3U8Downloader.Model;
using Microsoft.Win32;

namespace M3U8Downloader.ViewModels
{
    public class DownloadViewModel : NotifyObject
    {
        #region Fields

        private static long count = 0;
        private readonly Dictionary<M3U8FileModel, CancellationTokenSource> _canceltokenDic;
        private string _curUrl = string.Empty;
        private DownloadHelper _downloader;
        private string _fixUrl = "";
        private bool _isAnalyzing = false;

        private bool _isKeepM3U8ContentSrcUrl = true;

        private string _m3U8Content = "";
        private SaveFileDialog _saveFileDialog;
        private string _savePath = "";
        private M3U8FileModel _selectedM3U8 = null;
        private M3U8Segment _selectedNode = null;

        private string _selectedNodeFullPath = "";

        private string _stateText = "";

        #endregion Fields

        #region Constructors

        public DownloadViewModel()
        {
            SavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyVideos), "M3U8\\");
            M3U8Source = new ObservableCollection<M3U8FileModel>();
            _canceltokenDic = new Dictionary<M3U8FileModel, CancellationTokenSource>();

            _downloader = new DownloadHelper();
            _downloader.DownloadProgressChanged += OnDownloadProgressChanged;

            CommandAnalyze = new WpfCommand(OnAnalyze, CanAnalyze);
            CommandViewPath = new WpfCommand<string>(OnViewPath, CanViewPath);
            CommandDownload = new WpfCommand(OnDownload, CanDownload);
            CommandAbortDownload = new WpfCommand(OnAbortDownload, CanAbortDownload);
            CommandCombine = new WpfCommand(OnCombine, CanCombine);

            CommandRefreshM3U8Content = new WpfCommand(OnRefreshM3U8Content, CanRefreshM3U8Content);
            CommandSaveM3U8ContentToFile = new WpfCommand(OnSaveContentToFile, CanSaveContentToFile);
        }

        #endregion Constructors

        #region Properties

        public bool CanEdit => _selectedM3U8 != null;

        public ICommand CommandAbortDownload { get; private set; }

        public ICommand CommandAnalyze { get; private set; }

        public ICommand CommandCombine { get; private set; }

        public ICommand CommandDownload { get; private set; }

        public ICommand CommandRefreshM3U8Content { get; private set; }

        public ICommand CommandSaveM3U8ContentToFile { get; private set; }

        public ICommand CommandViewPath { get; private set; }

        public string CurrUrl
        {
            get => _curUrl;
            set
            {
                if (SetProperty(ref _curUrl, value))
                {
                    RaisePropertyChanged(nameof(IsLocalFile));
                }
            }
        }

        public string FixUrl
        {
            get => _fixUrl;
            set => SetProperty(ref _fixUrl, value);
        }

        public bool IsKeepM3U8ContentSrcUrl
        {
            get => _isKeepM3U8ContentSrcUrl;
            set => SetProperty(ref _isKeepM3U8ContentSrcUrl, value);
        }

        public bool IsLocalFile => File.Exists(CurrUrl);

        public string M3U8Content
        {
            get => _m3U8Content;
            set => SetProperty(ref _m3U8Content, value);
        }

        public ObservableCollection<M3U8FileModel> M3U8Source { get; private set; }

        public string SavePath
        {
            get => _savePath;
            set => SetProperty(ref _savePath, value);
        }

        public M3U8FileModel SelectedM3U8
        {
            get => _selectedM3U8;
            set
            {
                if (SetProperty(ref _selectedM3U8, value))
                {
                    StateText = "";
                    M3U8Content = "";
                    RaisePropertyChanged(nameof(CanEdit));
                }
            }
        }

        public M3U8Segment SelectedNode
        {
            get => _selectedNode;
            set
            {
                if (SetProperty(ref _selectedNode, value))
                {
                    if (_selectedNode != null)
                        SelectedNodeFullPath = Path.Combine(SelectedM3U8.SavePath, _selectedNode.SegmentName);
                }
            }
        }

        public string SelectedNodeFullPath
        {
            get => _selectedNodeFullPath;
            private set => SetProperty(ref _selectedNodeFullPath, value);
        }

        public string StateText
        {
            get => _stateText;
            set => SetProperty(ref _stateText, value);
        }

        #endregion Properties

        #region Methods

        private bool CanAbortDownload()
        {
            return SelectedM3U8 != null && SelectedM3U8.IsDownloading;
        }

        private bool CanAnalyze()
        {
            return !_isAnalyzing && !string.IsNullOrWhiteSpace(CurrUrl);
        }

        private bool CanCombine()
        {
            return !string.IsNullOrWhiteSpace(SelectedM3U8?.CombinedFile);
        }

        private bool CanDownload()
        {
            return SelectedM3U8 != null && !SelectedM3U8.IsDownloading;
        }

        private bool CanRefreshM3U8Content()
        {
            return true;
        }

        private bool CanSaveContentToFile()
        {
            return !string.IsNullOrEmpty(M3U8Content);
        }

        private bool CanViewPath(string path)
        {
            return File.Exists(path) || Directory.Exists(path);
        }

        private void OnAbortDownload()
        {
            if (SelectedM3U8 == null)
                return;
            var token = _canceltokenDic[SelectedM3U8];
            token.Cancel();
        }

        private async void OnAnalyze()
        {
            if (string.IsNullOrWhiteSpace(CurrUrl))
                return;
            _isAnalyzing = true;
            StateText = $"Analyzing : {CurrUrl}";
            try
            {
                if (File.Exists(CurrUrl))
                {
                    var uri = new Uri(FixUrl);
                    var content = File.ReadAllText(CurrUrl);
                    var m3 = M3U8FileAnalyzer.AnalyzeM3U8Content(uri, content);
                    var mm = new M3U8FileModel(m3)
                    {
                        SavePath = Path.GetDirectoryName(CurrUrl)
                    };
                    M3U8Source.Add(mm);
                }
                else
                {
                    var m3s = await Task.Factory.StartNew(() => M3U8FileAnalyzer.AnalyzeM3U8Url(CurrUrl));
                    foreach (var m in m3s)
                    {
                        count++;
                        var mm = new M3U8FileModel(m)
                        {
                            SavePath = Path.Combine(SavePath, $"{DateTime.Now.ToString("yyyyMMddHHmmss")}")
                        };
                        var rcount = 1;
                        var ofile = Path.GetFileNameWithoutExtension(mm.DisplayName);
                        while (M3U8Source.Any(p => p.DisplayName == mm.DisplayName))
                        {
                            mm.DisplayName = ofile + $"({rcount})" + Path.GetExtension(mm.DisplayName);
                            rcount++;
                        }
                        M3U8Source.Add(mm);
                    }
                }
                StateText = $"Analyzed : {CurrUrl}";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning($"Analyze Url Error. {ex.Message} ");
                StateText = $"Analyze Url Error. {ex.Message} ";
            }

            _isAnalyzing = false;
        }

        private void OnCombine()
        {
            var model = SelectedM3U8;
            OnCombineM3U8Segments(model);
        }

        private async void OnCombineM3U8Segments(M3U8FileModel model)
        {
            var rt = await M3U8Helper.CombineM3U8Segments(model.SourceTarget, model.SavePath, model.CombinedFile, OnCombineProgressChanged, model.IgnoreCombineError);
            var msg = $"Combine: {rt.IsComplete}";
            StateText = $"{model.SavePath} Combine: {rt.IsComplete}";
            if (rt.IsComplete)
            {
                msg = $"{DateTime.Now} Combine Complete!";
            }
            else
            {
                msg = $"{DateTime.Now} Combine Error, Ex:{rt.Exception?.Message}";
            }
            model.StateText = msg;
        }

        private void OnCombineProgressChanged(M3U8SegmentsCombineProgressChangedEventArgs e)
        {
            var model = M3U8Source.FirstOrDefault(p => p.SourceTarget == e.M3U8File);
            var msg = $"Combine : {e.CurrIndex} / {e.M3U8File.Segments.Length} ";
            model.StateText = msg;
            if (model == SelectedM3U8)
                StateText = $"{SelectedM3U8.SavePath} Combine : {e.CurrIndex} / {e.M3U8File.Segments.Length} ";
        }

        private async void OnDownload()
        {
            var model = SelectedM3U8;
            var path = model.SavePath;
            Directory.CreateDirectory(path);
            var target = model.SourceTarget;
            var token = new CancellationTokenSource();
            _canceltokenDic[model] = token;
            model.IsDownloading = true;
            var rt = await _downloader.DownloadM3U8VideoFilesAsync(target, model.SavePath, token.Token, model.IsSkipExistFile);
            if (rt.IsComplete)
            {
                model.StateText = $"{DateTime.Now} Download Complete!";

                StateText = $"{DateTime.Now} {model.DisplayName} Download Complete! [{path}]";
            }
            else
            {
                model.StateText = $"{DateTime.Now} Download Error, Ex:{rt.Exception?.Message}";

                StateText = $"{DateTime.Now} {model.DisplayName} Download Error! [{path}]";
            }
        }

        private void OnDownloadProgressChanged(object sender, M3U8DownloadProgressChangedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                var model = M3U8Source.FirstOrDefault(p => p.SourceTarget == e.M3U8File);
                model.StateText = $"{DateTime.Now} Download {e.LastIndex} / {e.M3U8File.Segments.Length} .";
                if (!e.IsInProgress)
                {
                    model.IsDownloading = false;
                    if (e.IsComplete && model.IsCombineAfterDownload)
                    {
                        SelectedM3U8.CombinedFile = SelectedM3U8.SavePath + ".ts";
                        OnCombineM3U8Segments(model);
                    }
                }
            }));
            if (e.LastIndex == -1)
            {
                if (e.M3U8File == SelectedM3U8.SourceTarget)
                {
                    var model = SelectedM3U8;
                    SelectedM3U8 = null;
                    SelectedM3U8 = model;
                }
                return;
            }

            if (SelectedM3U8.SourceTarget == e.M3U8File)
            {
                SelectedNode = SelectedM3U8.Segments[e.LastIndex];
                StateText = $"Download {e.M3U8File.Name} {e.LastIndex + 1}/{e.M3U8File.Segments.Length} InProgress:{e.IsInProgress}, IsComplete:{e.IsComplete}, IsCancelled:{e.IsCancelled}.";
            }
        }

        private void OnRefreshM3U8Content()
        {
            if (SelectedM3U8 != null)
            {
                M3U8Content = M3U8Helper.GetM3U8Content(SelectedM3U8.SourceTarget, IsKeepM3U8ContentSrcUrl);
            }
        }

        private void OnSaveContentToFile()
        {
            _saveFileDialog = new SaveFileDialog
            {
                DefaultExt = ".m3u8",
                CheckPathExists = true,
                OverwritePrompt = true,
                Filter = "M3U8列表文件|*.m3u8",
                Title = "另存为"
            };
            _saveFileDialog.InitialDirectory = SavePath;
            _saveFileDialog.FileName = DateTime.Now.ToString("yyyyMMddHHmmss");
            if (_saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllText(_saveFileDialog.FileName, M3U8Content);
                    SelectedM3U8.StateText = $"Save Content Success:{_saveFileDialog.FileName}";
                    StateText = $"Save Content Success:{_saveFileDialog.FileName}";
                }
                catch (Exception ex)
                {
                    SelectedM3U8.StateText = $"Save Content Error:{ex.Message}";
                    StateText = $"Save Content Error:{ex.Message}";
                }
            }
        }

        private void OnViewPath(string path)
        {
            if (File.Exists(path))
            {
                try
                {
                    Process.Start("Explorer.exe", $"/select,\"{path}\"");
                }
                catch { }
            }
            else if (Directory.Exists(path))
            {
                try
                {
                    Process.Start("Explorer.exe", $"\"{path}\"");
                }
                catch { }
            }
            else
            {
                StateText = $"Path {path} Not Exist!";
            }
        }

        #endregion Methods
    }
}