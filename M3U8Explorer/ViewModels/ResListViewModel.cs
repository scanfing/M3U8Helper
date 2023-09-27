using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M3U8Helper.Core;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;

namespace M3U8Explorer.ViewModels
{
    public class ResListViewModel : ObservableObject
    {
        #region Fields

        private ObservableCollection<M3U8ResourceInfo> _resInfos = new ObservableCollection<M3U8ResourceInfo>();

        #endregion Fields

        #region Constructors

        public ResListViewModel()
        {
            DownloadResCommand = new RelayCommand<M3U8ResourceInfo>(OnRequestDownloadM3u8, CanDownload);
        }

        #endregion Constructors

        #region Properties

        public ObservableCollection<M3U8ResourceInfo> ResourceInfos
        {
            get => _resInfos;
            set => SetProperty(ref _resInfos, value);
        }

        public RelayCommand<M3U8ResourceInfo> DownloadResCommand { get; private set; }

        #endregion Properties

        #region Methods

        private void OnRequestDownloadM3u8(M3U8ResourceInfo info)
        {
            var exJsonStr = JsonConvert.SerializeObject(info);
            var downloadfilePath = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), "M3U8Downloader.exe");
            if (!File.Exists(downloadfilePath))
                return;
            Process.Start(downloadfilePath, $"--json={exJsonStr}");
        }

        private bool CanDownload(M3U8ResourceInfo obj)
        {
            return true;
        }

        #endregion Methods
    }
}