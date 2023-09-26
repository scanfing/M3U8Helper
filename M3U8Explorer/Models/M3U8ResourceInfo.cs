using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace M3U8Explorer.Models
{
    public class M3U8ResourceInfo
    {
        #region Constructors

        public M3U8ResourceInfo()
        {
        }

        #endregion Constructors

        #region Properties

        public string Url { get; set; }

        public Dictionary<string, string> RequestHeaders { get; set; } = new Dictionary<string, string>();

        #endregion Properties
    }
}