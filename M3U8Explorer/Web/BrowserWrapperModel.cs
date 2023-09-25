using M3U8Downloader.Infrastruction;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace M3U8Explorer.Web
{
    public class BrowserWrapperModel : NotifyObject
    {
        #region Fields

        private WebView2 _webView;

        private string _title = "新建标签页";

        #endregion Fields

        #region Constructors

        public BrowserWrapperModel(WebView2 webbrowser)
        {
            _webView = webbrowser;
            webbrowser.NavigationCompleted += Webbrowser_NavigationCompleted;
        }

        #endregion Constructors

        #region Events

        public event EventHandler<CoreWebView2NewWindowRequestedEventArgs> NewWindowRequested;

        #endregion Events

        #region Properties

        public WebView2 WebView2 => _webView;

        public string Title
        {
            get => _title;
            private set => SetProperty(ref _title, value);
        }

        #endregion Properties

        #region Methods

        private void Webbrowser_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            var webbrowser = (WebView2)sender;
            webbrowser.NavigationCompleted -= Webbrowser_NavigationCompleted;
            Title = webbrowser.CoreWebView2.DocumentTitle;

            webbrowser.CoreWebView2.NewWindowRequested -= CoreWebView2_NewWindowRequested;
            webbrowser.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
            webbrowser.CoreWebView2.DocumentTitleChanged += CoreWebView2_DocumentTitleChanged;
        }

        private void CoreWebView2_DocumentTitleChanged(object sender, object e)
        {
            Title = WebView2.CoreWebView2.DocumentTitle;
        }

        private void CoreWebView2_NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            NewWindowRequested?.Invoke(this, e);
        }

        #endregion Methods
    }
}