namespace Chpath_Converter.Classes {

    public struct ChPoint {
        public float Distance;
        public float X;
        public float Y;
        public float Z;

        public static ChPoint Read(BinaryReader br) {
            float distance = br.ReadSingle();
            float x = br.ReadSingle();
            float y = br.ReadSingle();
            float z = br.ReadSingle();

            return new ChPoint { Distance = distance, X = x, Y = y, Z = z };
        }
    }
}