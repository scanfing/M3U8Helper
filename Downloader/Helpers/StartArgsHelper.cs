using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M3U8Downloader.Helpers
{
    public class StartArgsHelper
    {

        private static string[] _startArgs=new string[] { };

        public static string[] StartArgs
        {
            get => _startArgs;
            set => _startArgs = value;
        }
    }
}
