using M3U8Downloader.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace M3U8Downloader.Views
{
    /// <summary>
    /// ExploreView.xaml 的交互逻辑
    /// </summary>
    public partial class ExploreView : Window
    {
        #region Fields

        private ExploreViewModel _viewModel;

        #endregion Fields

        #region Constructors

        public ExploreView()
        {
            InitializeComponent();
            _viewModel = DataContext as ExploreViewModel;
            _viewModel.Browser = webBorwser;

            Loaded += ExploreView_Loaded;
        }

        #endregion Constructors

        #region Methods

        private void ExploreView_Loaded(object sender, RoutedEventArgs e)
        {
            _viewModel?.SetupBrowser();
        }

        #endregion Methods
    }
}