using M3U8Downloader.Infrastruction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace M3U8Downloader.ViewModels
{
    public class ExploreViewModel : NotifyObject
    {
        #region Constructors

        public ExploreViewModel()
        {
            NavigateToCommand = new WpfCommand<string>(OnRequestOpenUrl, (e) => true);
        }

        #endregion Constructors

        #region Properties

        public WebBrowser Browser { get; set; } = new WebBrowser();

        public ICommand NavigateToCommand { get; private set; }

        #endregion Properties

        #region Methods

        public void SetupBrowser()
        {
            Browser.Navigated += Browser_Navigated;
            Browser.LoadCompleted += Browser_LoadCompleted;
        }

        private void Browser_LoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            System.Diagnostics.Trace.TraceInformation($"LoadCompleted {e.Uri}");
        }

        private void Browser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            System.Diagnostics.Trace.TraceInformation($"Navigated {e.Uri}");
        }

        private void OnRequestOpenUrl(string url)
        {
            Browser.Navigate(url);
        }

        #endregion Methods
    }
}