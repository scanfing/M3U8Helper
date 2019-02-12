using System.Collections.Generic;
using System.Linq;
using AuxiliaryTools.M3U8;
using M3U8Downloader.Infrastruction;

namespace M3U8Downloader.Model
{
    public class M3U8FileModel : NotifyObject
    {
        #region Fields

        public readonly M3U8File SourceTarget;
        private string _combinedfile = "";
        private string _displayName = "";
        private bool _ignoreCombineError = false;
        private bool _isDownloading = false;
        private string _savepath = "";
        private string _stateText = "";

        #endregion Fields

        #region Constructors

        public M3U8FileModel(M3U8File m3u)
        {
            SourceTarget = m3u;
            Head = m3u.Head;
            Segments = m3u.Segments.ToList();
        }

        #endregion Constructors

        #region Properties

        public string CombinedFile
        {
            get => _combinedfile;
            set => SetProperty(ref _combinedfile, value);
        }

        public string DisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(_displayName))
                {
                    return SourceTarget.Name;
                }
                return _displayName;
            }
            set => SetProperty(ref _displayName, value);
        }

        public M3U8Head Head { get; private set; }

        public bool IgnoreCombineError
        {
            get => _ignoreCombineError;
            set => SetProperty(ref _ignoreCombineError, value);
        }

        public bool IsDownloading
        {
            get => _isDownloading;
            set => SetProperty(ref _isDownloading, value);
        }

        public string SavePath
        {
            get => _savepath;
            set => SetProperty(ref _savepath, value);
        }

        public List<M3U8Segment> Segments { get; private set; }
        public string SourceUrl { get; private set; }

        public string StateText
        {
            get => _stateText;
            set => SetProperty(ref _stateText, value);
        }

        #endregion Properties
    }
}