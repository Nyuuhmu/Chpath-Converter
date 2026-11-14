namespace Chpath_Converter.ChPath {

    public class ChPath {
        public byte[] HeaderIdentifierBytes;
        public int ByteCount;
        public int Unknown03;                   // always 256
        public byte[] UnknownBytes04;           // always { 12, 0, 0, 0 }
        public int PathCount;
        public byte[] UnknownBytes06;           // always { 0, 0, 1, 0 }
        public List<Path> Paths;
    }

    public class Path {
        public bool NewVersion;
        public int Unknown01;
        public byte[] UnknownBytes02;
        public int Unknown03;
        public byte[] UnknownBytes04;
        public short Unknown05;                 // always 2?
        public short PathByteCount;             // somehow related to point count, bytes? +68
        public int Unknown07;
        public byte[] UnknownBytes08;
        public float End1;                      // end?
        public float End2;                      // end? same as unk4
        public int Unknown11;
        public int PointByteCount;              // byte length for points? (unk7 * 16)
        public int PointCount;                  // point count
        public int VersionDate;
        public int Unknown15;

        public List<Point> Points;

        public static Path Read(System.IO.BinaryReader br, bool last) {
            bool newVersion = false;
            int unk01 = br.ReadInt32();
            byte[] unk02 = br.ReadBytes(4);
            int unk03 = br.ReadInt32();
            byte[] unk04 = br.ReadBytes(10);
            short unk05 = br.ReadInt16();
            short pathByteCount = br.ReadInt16();
            int unk07 = br.ReadInt32();
            byte[] unk08 = br.ReadBytes(38);
            float end1 = br.ReadSingle();
            float end2 = br.ReadSingle();
            int unk11 = br.ReadInt32();
            int pointByteCount = br.ReadInt32();
            int pointCount = br.ReadInt32();
            int versionDate = -1;
            int unk15 = -1;

            List<Point> points = new List<Point>();
            for (int j = 0; j < pointCount; j++) { points.Add(Point.Read(br)); }

            byte[] checkByte = br.ReadBytes(1);
            if (checkByte.Length > 0) {
                if (checkByte[0] == 0xD6) { // Old Version
                    byte[] restBytes = br.ReadBytes(11);
                    versionDate = BitConverter.ToInt32(checkByte.Concat(restBytes).ToArray());

                    if (!last) { unk15 = br.ReadInt32(); }
                } else { // New Version
                    newVersion = true;
                    byte[] restBytes = br.ReadBytes(3);
                    unk15 = BitConverter.ToInt32(checkByte.Concat(restBytes).ToArray());
                }
            }

            return new Path { NewVersion = newVersion, Unknown01 = unk01, UnknownBytes02 = unk02, Unknown03 = unk03, UnknownBytes04 = unk04, Unknown05 = unk05, PathByteCount = pathByteCount, Unknown07 = unk07, UnknownBytes08 = unk08, End1 = end1, End2 = end2, Unknown11 = unk11, PointByteCount = pointByteCount, PointCount = pointCount, VersionDate = versionDate, Unknown15 = unk15, Points = points };
        }
    }

    public class Point {
        public float Distance;
        public float X;
        public float Y;
        public float Z;

        public static Point Read(System.IO.BinaryReader br) {
            float distance = br.ReadSingle();
            float x = br.ReadSingle();
            float y = br.ReadSingle();
            float z = br.ReadSingle();

            return new Point { Distance = distance, X = x, Y = y, Z = z };
        }
    }

}
