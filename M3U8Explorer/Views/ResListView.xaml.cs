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
using M3U8Explorer.Models;
using M3U8Explorer.ViewModels;

namespace M3U8Explorer.Views
{
    /// <summary>
    /// ResListView.xaml 的交互逻辑
    /// </summary>
    public partial class ResListView : Window
    {
        private ResListViewModel _viewModel;
        public ResListView(IEnumerable<M3U8ResourceInfo> resList)
        {
            InitializeComponent();
            _viewModel =DataContext as ResListViewModel;

            _viewModel.ResourceInfos.Clear();
            foreach(var resourceInfo in resList)
            {
                _viewModel.ResourceInfos.Add(resourceInfo);
            }
        }
    }
}
