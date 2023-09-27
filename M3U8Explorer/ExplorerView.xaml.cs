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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace M3U8Explorer
{
    /// <summary>
    /// ExplorerView.xaml 的交互逻辑
    /// </summary>
    public partial class ExplorerView : Window
    {
        #region Fields

        private ExplorerViewModel viewModel;

        #endregion Fields

        #region Constructors

        public ExplorerView()
        {
            InitializeComponent();
            viewModel = DataContext as ExplorerViewModel;
            viewModel.Dispatcher = Dispatcher;
        }

        #endregion Constructors

        #region Methods

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            viewModel.CreateNewBrowser("https://v.cctv.com/");
        }

        #endregion Methods
    }
}