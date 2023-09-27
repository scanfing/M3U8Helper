using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
    public class ExplorerViewModel : ObservableObject
    {
        #region Fields

        private BrowserModel _currentBrowser;

        #endregion Fields

        #region Constructors

        public ExplorerViewModel()
        {
            Browsers = new ObservableCollection<BrowserModel>();

            CloseBorwserPageCommand = new RelayCommand<BrowserModel>(OnRequestClosePage, (e) => true);
        }

        #endregion Constructors

        #region Properties

        public ObservableCollection<BrowserModel> Browsers { get; private set; }

        public ICommand CloseBorwserPageCommand { get; private set; }

        public BrowserModel CurrentBrowser
        {
            get => _currentBrowser;
            set => SetProperty(ref _currentBrowser, value);
        }

        public Dispatcher Dispatcher { get; set; }

        #endregion Properties

        #region Methods

        public BrowserModel CreateNewBrowser(string url)
        {
            WebView2 browser = new WebView2();
            var wrapper = new BrowserModel(browser);
            InitBorwser(wrapper);
            Browsers.Add(wrapper);
            CurrentBrowser = wrapper;
            browser.Source = new Uri(url);
            return wrapper;
        }

        private void InitBorwser(BrowserModel webbrowser)
        {
            webbrowser.NewWindowRequested += WebView2_NewWindowRequested;
        }

        private void OnRequestClosePage(BrowserModel webbrowser)
        {
            if (webbrowser is null)
                return;

            Browsers.Remove(webbrowser);
        }

        private void WebView2_NewWindowRequested(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NewWindowRequestedEventArgs e)
        {
            var browser = CreateNewBrowser(e.Uri);
            e.Handled = true;
        }

        #endregion Methods
    }
}