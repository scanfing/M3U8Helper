using M3U8Downloader.Infrastruction;
using M3U8Explorer.Web;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace M3U8Explorer
{
    public class ExplorerViewModel : NotifyObject
    {
        #region Fields

        private BrowserWrapperModel _currentBrowser;

        #endregion Fields

        #region Constructors

        public ExplorerViewModel()
        {
            Browsers = new ObservableCollection<BrowserWrapperModel>();

            CloseBorwserPageCommand = new WpfCommand<BrowserWrapperModel>(OnRequestClosePage, (e) => true);
        }

        #endregion Constructors

        #region Properties

        public ICommand CloseBorwserPageCommand { get; private set; }

        public Dispatcher Dispatcher { get; set; }

        public ObservableCollection<BrowserWrapperModel> Browsers { get; private set; }

        public BrowserWrapperModel CurrentBrowser
        {
            get => _currentBrowser;
            set => SetProperty(ref _currentBrowser, value);
        }

        #endregion Properties

        #region Methods

        public BrowserWrapperModel CreateNewBrowser(string url)
        {
            WebView2 browser = new WebView2();
            var wrapper = new BrowserWrapperModel(browser);
            InitBorwser(wrapper);
            Browsers.Add(wrapper);
            CurrentBrowser = wrapper;
            browser.Source = new Uri(url);
            return wrapper;
        }

        private void OnRequestClosePage(BrowserWrapperModel webbrowser)
        {
            if (webbrowser is null)
                return;

            Browsers.Remove(webbrowser);
        }

        private void InitBorwser(BrowserWrapperModel webbrowser)
        {
            webbrowser.NewWindowRequested += WebView2_NewWindowRequested;
        }

        private void WebView2_NewWindowRequested(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NewWindowRequestedEventArgs e)
        {
            var browser = CreateNewBrowser(e.Uri);
            e.Handled = true;
        }

        #endregion Methods
    }
}