namespace Chpath_Converter.Classes {

    public struct ChPathFile {
        public byte[] HeaderIdentifierBytes;
        public int ByteCount;
        public int Unknown03;           // always 256
        public byte[] UnknownBytes04;   // always { 12, 0, 0, 0 }
        public int PathCount;
        public byte[] UnknownBytes06;   // always { 0, 0, 1, 0 }
        public List<ChPath> Paths;
    }
}