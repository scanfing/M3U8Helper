using System;
using System.Collections.Generic;
using System.Linq;

namespace AuxiliaryTools.M3U8
{
    public class M3U8File
    {
        #region Fields

        private List<M3U8Segment> _nodes;

        #endregion Fields

        #region Constructors

        public M3U8File(Uri url, M3U8Head head, ICollection<M3U8Segment> nodes)
        {
            RESOLUTION = "";
            Head = head;
            _nodes = new List<M3U8Segment>(nodes);
            SourceUrl = url;
            Name = SourceUrl.LocalPath.Substring(SourceUrl.LocalPath.LastIndexOf('/') + 1);
            TotalSeconds = _nodes.Sum(p => p.Seconds);
            TotalTime = TimeSpan.FromSeconds(TotalSeconds);
        }

        #endregion Constructors

        #region Properties

        public int BANDWIDTH { get; set; }

        public M3U8Head Head { get; private set; }

        public string Name { get; private set; }

        public string RESOLUTION { get; set; }

        public M3U8Segment[] Segments => _nodes.ToArray();

        public Uri SourceUrl { get; private set; }

        public double TotalSeconds { get; private set; }

        public TimeSpan TotalTime { get; private set; }

        #endregion Properties

        #region Methods

        public override string ToString()
        {
            return $"{SourceUrl}[{_nodes.Count}], {BANDWIDTH}, {RESOLUTION}, {TotalTime}";
        }

        #endregion Methods
    }
}