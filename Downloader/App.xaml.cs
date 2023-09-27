using M3U8Downloader.Helpers;
using System.Windows;

namespace M3U8Downloader
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            StartArgsHelper.StartArgs = e.Args;
            base.OnStartup(e);
        }
    }
}