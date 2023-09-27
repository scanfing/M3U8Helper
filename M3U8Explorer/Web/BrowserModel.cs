using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M3U8Helper.Core;
using M3U8Explorer.Views;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace M3U8Explorer.Web
{
    public class BrowserModel : ObservableObject
    {
        #region Fields

        private string _title = "新建标签页";
        private WebView2 _webView;

        #endregion Fields

        #region Constructors

        public BrowserModel(WebView2 webbrowser)
        {
            M3U8Resources = new ObservableCollection<M3U8ResourceInfo>();

            _webView = webbrowser;
            webbrowser.CoreWebView2InitializationCompleted += Webbrowser_CoreWebView2InitializationCompleted;
            webbrowser.NavigationStarting += Webbrowser_NavigationStarting;
            webbrowser.NavigationCompleted += Webbrowser_NavigationCompleted;

            GoBackCommand = new RelayCommand(OnRequestBack, () => WebView2.CanGoBack);
            GoForwardCommand = new RelayCommand(OnRequestForward, () => WebView2.CanGoForward);
            RefreshCommand = new RelayCommand(OnRequestRefresh, () => WebView2.IsLoaded);
            OpenUrlInCurrBrowserCommand = new RelayCommand<string>(OnRequestOpenUrlInCurrBrowser, (e) => true);
            OpenResListViewCommand = new RelayCommand<ObservableCollection<M3U8ResourceInfo>>(OnRequestOpResList, (e) => true);
        }

        #endregion Constructors

        #region Events

        public event EventHandler<CoreWebView2NewWindowRequestedEventArgs> NewWindowRequested;

        #endregion Events

        #region Properties

        public RelayCommand GoBackCommand { get; private set; }

        public RelayCommand GoForwardCommand { get; private set; }

        public ObservableCollection<M3U8ResourceInfo> M3U8Resources { get; private set; }

        public RelayCommand<ObservableCollection<M3U8ResourceInfo>> OpenResListViewCommand { get; private set; }

        public RelayCommand<string> OpenUrlInCurrBrowserCommand { get; private set; }

        public RelayCommand RefreshCommand { get; private set; }

        public string Title
        {
            get => _title;
            private set => SetProperty(ref _title, value);
        }

        public WebView2 WebView2 => _webView;

        #endregion Properties

        #region Methods

        public void OnRequestForward()
        {
            if (!WebView2.CanGoForward)
                return;
            WebView2.GoForward();
        }

        public void OnRequestRefresh()
        {
            WebView2.Reload();
        }

        private void CoreWebView2_DocumentTitleChanged(object sender, object e)
        {
            Title = WebView2.CoreWebView2.DocumentTitle;
            RefreshButtonState();
        }

        private void CoreWebView2_NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            if (e.Uri.Equals("about:blank"))
            {
                e.Handled = true;
                return;
            }
            NewWindowRequested?.Invoke(this, e);
        }

        private void CoreWebView2_WebResourceRequested(object sender, CoreWebView2WebResourceRequestedEventArgs e)
        {
            System.Diagnostics.Trace.WriteLine($"Web Resource Request: {e.Request.Uri}");
            if (!e.Request.Uri.ToLower().Contains(".m3u8"))
            {
                return;
            }
            var m3u8r = new M3U8ResourceInfo();
            m3u8r.Url = e.Request.Uri;
            foreach (var kv in e.Request.Headers)
            {
                m3u8r.RequestHeaders[kv.Key] = kv.Value;
            }

            M3U8Resources.Add(m3u8r);
        }

        private void CoreWebView2_WebResourceResponseReceived(object sender, CoreWebView2WebResourceResponseReceivedEventArgs e)
        {
            System.Diagnostics.Trace.WriteLine($"Web Resource Response: {e.Request.Uri}");
        }

        private void OnRequestBack()
        {
            if (!WebView2.CanGoBack)
                return;
            WebView2.GoBack();
        }

        private void OnRequestOpenUrlInCurrBrowser(string url)
        {
            var dstUrl = url;
            if (!url.StartsWith("http"))
            {
                dstUrl = $"http://{url}";
            }
            WebView2.CoreWebView2.Navigate(dstUrl);
        }

        private void OnRequestOpResList(ObservableCollection<M3U8ResourceInfo> enumerable)
        {
            var resWin = new ResListView(enumerable);
            resWin.Owner = Application.Current.MainWindow;
            resWin.ShowDialog();
        }

        private void RefreshButtonState()
        {
            GoBackCommand.NotifyCanExecuteChanged();
            GoForwardCommand.NotifyCanExecuteChanged();
            RefreshCommand.NotifyCanExecuteChanged();
        }

        private void Webbrowser_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
                return;

            WebView2.CoreWebView2.NewWindowRequested -= CoreWebView2_NewWindowRequested;
            WebView2.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
            WebView2.CoreWebView2.DocumentTitleChanged -= CoreWebView2_DocumentTitleChanged;
            WebView2.CoreWebView2.DocumentTitleChanged += CoreWebView2_DocumentTitleChanged;

            WebView2.CoreWebView2.WebResourceRequested += CoreWebView2_WebResourceRequested;
            WebView2.CoreWebView2.WebResourceResponseReceived += CoreWebView2_WebResourceResponseReceived;

            WebView2.CoreWebView2.AddWebResourceRequestedFilter("*.m3u8", CoreWebView2WebResourceContext.All);
            WebView2.CoreWebView2.AddWebResourceRequestedFilter("*.ts", CoreWebView2WebResourceContext.All);
        }

        private void Webbrowser_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            var webbrowser = (WebView2)sender;

            Title = webbrowser.CoreWebView2.DocumentTitle;
            RefreshButtonState();
        }

        private void Webbrowser_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            M3U8Resources.Clear();
            RefreshButtonState();
        }

        #endregion Methods
    }
}