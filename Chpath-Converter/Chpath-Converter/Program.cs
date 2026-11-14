using System.Globalization;
using System.Numerics;

namespace Chpath_Converter {

    class Program {

        static void Main(string[] _args) {
            if (_args.Length == 0) {
                Console.WriteLine("Drag a .chpath or .index file onto this .exe file to start the conversion.");
                Console.Read();

                return;
            }

            CultureInfo.CurrentCulture = new CultureInfo("en-US");

            foreach (string arg in _args) {
                try {
                    if (System.IO.Path.GetExtension(arg) == ".chpath") {
                        ChPath.ChPath chPath = ImportChpathFile(arg);
                        ExportToIndexFile(chPath, arg);
                    }

                    if (System.IO.Path.GetExtension(arg) == ".index") {
                        Index.Index index = ImportIndexFile(arg);
                        ExportToChpathFile(ConvertIndexToChPath(index), arg);
                    }
                } catch (Exception ex) {
                    Console.WriteLine($"FAILED on '{arg}':");
                    Console.WriteLine(ex);   // type, message and line numbers
                }

            }

            Console.WriteLine();
            Console.WriteLine("Press enter to exit...");
            Console.Read();
        }


        // ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public static ChPath.ChPath ImportChpathFile(string _filepath) {
            using (System.IO.BinaryReader br = new System.IO.BinaryReader(System.IO.File.OpenRead(_filepath))) {
                byte[] headerIdentifierBytes = br.ReadBytes(25);
                int byteCount = br.ReadInt32();
                int unk03 = br.ReadInt32();            // always 256
                byte[] unk04 = br.ReadBytes(4);        // always 12
                int pathCount = br.ReadInt32();
                byte[] unk06 = br.ReadBytes(4);        // always 65536

                List<ChPath.Path> paths = new List<ChPath.Path>();
                for (int i = 0; i < pathCount; i++) { paths.Add(ChPath.Path.Read(br, i == (pathCount - 1))); }

                ChPath.ChPath chPath = new ChPath.ChPath();
                chPath.HeaderIdentifierBytes = headerIdentifierBytes;
                chPath.ByteCount = byteCount;
                chPath.Unknown03 = unk03;
                chPath.UnknownBytes04 = unk04;
                chPath.PathCount = pathCount;
                chPath.UnknownBytes06 = unk06;
                chPath.Paths = paths;
                
                return chPath;
            }
        }

        public static Index.Index ImportIndexFile(string _filepath) {
            Index.Index index = new Index.Index();

            // Read and Prepare File
            string[] fileLines = File.ReadAllLines(_filepath);
            fileLines = fileLines.Skip(3).ToArray();

            for (int i = 0; i < fileLines.Length; i++) {
                fileLines[i] = fileLines[i].Trim();
            }

            // Parse and Skip Path Count
            int pathCount = int.Parse(fileLines[0].Split(' ')[0]);
            fileLines = fileLines.Skip(1).ToArray();

            // Import
            for (int i = 0; i < pathCount; i++) {
                string[] vars = fileLines[0].Split(' ');
                fileLines = fileLines.Skip(1).ToArray();

                Index.Path indexPath = new Index.Path() {
                    pathID = int.Parse(vars[0]),
                    start = float.Parse(vars[1], CultureInfo.InvariantCulture.NumberFormat),
                    end = float.Parse(vars[2], CultureInfo.InvariantCulture.NumberFormat),
                    speed = float.Parse(vars[3], CultureInfo.InvariantCulture.NumberFormat),
                    step = float.Parse(vars[4], CultureInfo.InvariantCulture.NumberFormat),
                    count = int.Parse(vars[5].Split('/')[0])
                };

                for (int j = 0; j < indexPath.count; j++) {
                    string[] pinPositions = fileLines[0].Split(' ');

                    Vector3 points = new Vector3(float.Parse(pinPositions[0]), float.Parse(pinPositions[1]), float.Parse(pinPositions[2]));
                    indexPath.Points.Add(points);

                    fileLines = fileLines.Skip(1).ToArray();
                }

                index.Paths.Add(indexPath);
            }

            return index;
        }

        // ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        public static ChPath.ChPath ConvertIndexToChPath(Index.Index _index) {
            ChPath.ChPath chPath = new ChPath.ChPath();
            chPath.Paths = new List<ChPath.Path>();

            chPath.HeaderIdentifierBytes = new byte[] { 0x43, 0x48, 0x50, 0x41, 0x54, 0x48, 0x00, 0x6A, 0xCC, 0x31, 0x01, 0x4E, 0x48, 0x4E, 0x2D, 0x41, 0x47, 0x00, 0x6A, 0x63, 0x00, 0x05, 0x00, 0x00, 0x00 };
            chPath.ByteCount = -1;
            chPath.Unknown03 = 256;
            chPath.UnknownBytes04 = new byte[] { 0x0C, 0x00, 0x00, 0x00 };
            chPath.PathCount = _index.Paths.Count;
            chPath.UnknownBytes06 = new byte[] { 0x00, 0x00, 0x01, 0x00 };

            foreach (var indexPath in _index.Paths) {
                ChPath.Path path = new ChPath.Path();

                path.NewVersion = false;
                path.Unknown01 = 24;
                path.UnknownBytes02 = new byte[] { 0x00, 0x10, 0x01, 0x00 };
                path.Unknown03 = 16;
                path.UnknownBytes04 = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                path.Unknown05 = 2;
                path.PathByteCount = -1;
                path.Unknown07 = 268435456;
                path.UnknownBytes08 = new byte[] { 0x02, 0x00, 0x14, 0x00, 0x00, 0x00, 0xD6, 0x56, 0x31, 0x01, 0xB7, 0x07, 0x00, 0x00, 0x01, 0x01, 0x00, 0x00, 0x00, 0x11, 0x02, 0x00, 0x1C, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                path.End1 = indexPath.end;
                path.End2 = indexPath.end;              // path.End1;
                path.Unknown11 = 139264;                // could be split in 2x int16
                path.PointByteCount = -1;
                path.PointCount = indexPath.Points.Count;

                List<ChPath.Point> points = new List<ChPath.Point>();

                // clanker code
                float[] raw = new float[indexPath.Points.Count];
                for (int i = 1; i < raw.Length; i++) {
                    raw[i] = raw[i - 1] + Vector3.Distance(indexPath.Points[i - 1], indexPath.Points[i]);
                }

                float total = raw.Length > 0 ? raw[^1] : 0f;
                float scale = total > 0f ? indexPath.end / total : 0f;

                for (int i = 0; i < raw.Length; i++) {
                    Vector3 p = indexPath.Points[i];
                    points.Add(new ChPath.Point { Distance = raw[i] * scale, X = p.X, Y = p.Y, Z = p.Z });
                }

                path.Points = points;

                path.VersionDate = -1;                  // 20010710
                path.Unknown15 = 65536;

                path.PathByteCount = CalculatePathByteCount(path);
                path.PointByteCount = CalculatePointByteCount(path);

                chPath.Paths.Add(path);
            }

            chPath.ByteCount = CalculateChPathByteCount(chPath);
            return chPath;
        }

        // ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public static void ExportToChpathFile(ChPath.ChPath _chPath, string _filepath = "") {
            _filepath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(_filepath) ?? "", System.IO.Path.GetFileNameWithoutExtension(_filepath) + "_converted.chpath");

            Console.WriteLine($"Writing to '{_filepath}'");

            using (System.IO.BinaryWriter bw = new System.IO.BinaryWriter(System.IO.File.Create(_filepath))) {
                bw.Write(_chPath.HeaderIdentifierBytes);
                bw.Write(_chPath.ByteCount);
                bw.Write(_chPath.Unknown03);
                bw.Write(_chPath.UnknownBytes04);
                bw.Write(_chPath.PathCount);
                bw.Write(_chPath.UnknownBytes06);

                for (int i = 0; i < _chPath.PathCount; i++) {
                    bw.Write(_chPath.Paths[i].Unknown01);
                    bw.Write(_chPath.Paths[i].UnknownBytes02);
                    bw.Write(_chPath.Paths[i].Unknown03);
                    bw.Write(_chPath.Paths[i].UnknownBytes04);
                    bw.Write(_chPath.Paths[i].Unknown05);
                    bw.Write(_chPath.Paths[i].PathByteCount);
                    bw.Write(_chPath.Paths[i].Unknown07);
                    bw.Write(_chPath.Paths[i].UnknownBytes08);
                    bw.Write(_chPath.Paths[i].End1);
                    bw.Write(_chPath.Paths[i].End2);
                    bw.Write(_chPath.Paths[i].Unknown11);
                    bw.Write(_chPath.Paths[i].PointByteCount);
                    bw.Write(_chPath.Paths[i].PointCount);

                    foreach (ChPath.Point point in _chPath.Paths[i].Points) {
                        bw.Write(point.Distance);
                        bw.Write(point.X);
                        bw.Write(point.Y);
                        bw.Write(point.Z);
                    }

                    bw.Write(new byte[] { 0xD6, 0x56, 0x31, 0x01, 0xB7, 0x07, 0x00, 0x00, 0x01, 0x01, 0x00, 0x00 });

                    if (i != _chPath.PathCount - 1) { bw.Write(_chPath.Paths[i].Unknown15); }
                }
            }
        }

        public static void ExportToIndexFile(ChPath.ChPath _chPath, string _filepath = "") {
            _filepath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(_filepath) ?? "", System.IO.Path.GetFileNameWithoutExtension(_filepath) + "_converted.index");

            Console.WriteLine($"Writing to '{_filepath}'");

            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(System.IO.File.Create(_filepath))) {
                sw.WriteLine($"{Spaces(2)}path4cull");
                sw.WriteLine($"{Spaces(2)}20050929");
                sw.WriteLine($"{Spaces(2)}sh");

                sw.WriteLine($"{Spaces(4)}{_chPath.PathCount}");

                for (int i = 0; i < _chPath.PathCount; i++) {
                    sw.WriteLine($"{Spaces(6)}{i} 0.000000 {_chPath.Paths[i].End1} 100.000000 1.000000 {_chPath.Paths[i].PointCount}");

                    for (int j = 0; j < _chPath.Paths[i].PointCount; j++) { sw.WriteLine($"{Spaces(8)} {_chPath.Paths[i].Points[j].X} {_chPath.Paths[i].Points[j].Y} {_chPath.Paths[i].Points[j].Z}"); }
                }
            }
        }


        // ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public static string Spaces(int _amount) { 
            return "".PadLeft(_amount); 
        }

        public static int CalculatePointByteCount(ChPath.Path _path) {
            int headerBytes = 12;
            int pointBytes = 16;
            int totalBytes = 0 + headerBytes;

            foreach (ChPath.Point point in _path.Points) { totalBytes += pointBytes; }

            return totalBytes;
        }

        public static short CalculatePathByteCount(ChPath.Path _path) {
            return (short)(CalculatePointByteCount(_path) + 68);
        }

        public static int CalculateChPathByteCount(ChPath.ChPath _chPath) {
            int headerBytes = 20;
            int bytesPerPoint = 16;
            int oldVersionPathBytes = 104;
            int newVersionPathBytes = 0;

            int totalBytes = 0 + headerBytes;
            for (int i = 0; i < _chPath.PathCount; i++) {
                totalBytes += _chPath.Paths[i].NewVersion ? newVersionPathBytes : oldVersionPathBytes;

                for (int j = 0; j < _chPath.Paths[i].PointCount; j++) {
                    totalBytes += bytesPerPoint;
                }
            }

            return totalBytes;
        }

    }
}