using System.Numerics;

namespace Chpath_Converter.Index {

    public class Index {
        public List<Path> Paths;

        public Index() {
            Paths = new List<Path>();
        }
    }

    public class Path {
        public int pathID = 0;
        public float start = 0f;
        public float end = 0f;
        public float speed = 0f;
        public float step = 0f;
        public int count = 0;
        public List<Vector3> Points;

        public Path() {
            Points = new List<Vector3>();
        }
    }
}
