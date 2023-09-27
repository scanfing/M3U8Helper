using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using M3U8Helper.Core;
using M3U8Explorer.ViewModels;

namespace M3U8Explorer.Views
{
    /// <summary>
    /// ResListView.xaml 的交互逻辑
    /// </summary>
    public partial class ResListView : Window
    {
        private ResListViewModel _viewModel;
        public ResListView(ObservableCollection<M3U8ResourceInfo> resList)
        {
            InitializeComponent();
            _viewModel =DataContext as ResListViewModel;

            _viewModel.ResourceInfos = resList;
        }
    }
}
