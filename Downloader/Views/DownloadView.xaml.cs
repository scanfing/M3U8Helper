using System;
using System.Windows;
using System.Windows.Forms;
using M3U8Downloader.ViewModels;

namespace M3U8Downloader.Views
{
    /// <summary>
    /// DownloadView.xaml 的交互逻辑
    /// </summary>
    public partial class DownloadView : Window
    {
        #region Constructors

        public DownloadView()
        {
            InitializeComponent();
            DownloadViewModel = DataContext as DownloadViewModel;
            Loaded += DownloadView_Loaded;
        }

        #endregion Constructors

        #region Properties

        private DownloadViewModel DownloadViewModel { get; set; }

        #endregion Properties

        #region Methods

        private void DownloadView_Loaded(object sender, RoutedEventArgs e)
        {
            DownloadViewModel.AnalyzeStartArgs();
        }

        private void Btn_Outputpath_Click(object sender, RoutedEventArgs e)
        {
            var opd = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "*.ts|TS",
                DefaultExt = "ts"
            };
            if (opd.ShowDialog() == true)
            {
                var db = TXT_CombineFile.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty);
                if (db != null)
                {
                    TXT_CombineFile.SetValue(System.Windows.Controls.TextBox.TextProperty, opd.FileName);
                    db.UpdateSource();
                }
            }
        }

        private void Btn_Savepath_Click(object sender, RoutedEventArgs e)
        {
            var opd = new FolderBrowserDialog();
            if (opd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var db = TXT_SavePath.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty);
                if (db != null)
                {
                    TXT_SavePath.SetValue(System.Windows.Controls.TextBox.TextProperty, opd.SelectedPath);
                    db.UpdateSource();
                }
            }
        }

        #endregion Methods
    }
}