namespace AuxiliaryTools.M3U8
{
    public class M3U8Head
    {
        #region Properties

        public string EncryptMethod { get; set; }

        public bool HasMap { get; set; }
        public bool IsEncrypt { get; set; }
        public string IV { get; set; }
        public byte[] Key { get; set; }
        public string KeyFile { get; set; }
        public string KeyUrl { get; set; }
        public string MapFile { get; set; }
        public string MapUrl { get; set; }
        public int MEDIA_SEQUENCE { get; set; } = 0;

        public double TARGETDURATION { get; set; }

        public string VERSION { get; set; }

        #endregion Properties

        #region Methods

        public override string ToString()
        {
            var str = $"#EXT-X-VERSION:{VERSION}\n#EXT-X-TARGETDURATION:{TARGETDURATION}\n#EXT-X-MEDIA-SEQUENCE:{MEDIA_SEQUENCE}";
            if (IsEncrypt)
                str += $"\n#EXT-X-KEY:METHOD={EncryptMethod}";
            return str;
        }

        #endregion Methods
    }
}