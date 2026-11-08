namespace Chpath_Converter.Classes {

    public class Converter {

        public static void Convert(FileStream _stream) {
            try {
                ExportToIndexFile(ImportFromChPathFile(_stream), Path.Combine(_stream.Name + $".index"));
            } catch { Console.WriteLine($"Error: {_stream} is an invalid file."); }
        }

        // ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        static ChPathFile ImportFromChPathFile(FileStream _stream) {
            using (BinaryReader br = new BinaryReader(_stream)) {
                byte[] headerIdentifierBytes = br.ReadBytes(25);
                int byteCount = br.ReadInt32();
                int unk03 = br.ReadInt32();            // always 256
                byte[] unk04 = br.ReadBytes(4);        // always 12
                int pathCount = br.ReadInt32();
                byte[] unk06 = br.ReadBytes(4);        // always 65536

                List<ChPath> paths = new List<ChPath>();
                for (int i = 0; i < pathCount; i++) { paths.Add(ChPath.Read(br, i == (pathCount - 1))); }

                ChPathFile chPath = new ChPathFile();
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

        // ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public static string Spaces(int _amount) {
            return "".PadLeft(_amount);
        }

        static void ExportToIndexFile(ChPathFile _chPath, string _filename) {
            using (StreamWriter sw = new StreamWriter(File.OpenWrite(_filename))) {
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
    }
}
