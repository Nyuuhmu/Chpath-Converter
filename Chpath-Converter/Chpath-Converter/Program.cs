using System.Globalization;
using Chpath_Converter.Classes;

namespace Chpath_Converter {

    class Program {

        static void Main(string[] _args) {
            if (_args.Length == 0) {
                Console.WriteLine("Drag a .chpath file onto this .exe file to start the conversion.");
                Console.Read();

                return;
            }

            CultureInfo.CurrentCulture = new CultureInfo("en-US");

            foreach (string arg in _args) {
                if (Path.GetExtension(arg) != ".chpath") {
                    Console.WriteLine($"Invalid file: \"{arg}\"");
                    continue;
                }

                Converter.Convert(File.OpenRead(arg));
                Console.WriteLine($"Successfully converted: \"{arg}\"");
            }

            Console.WriteLine();
            Console.WriteLine("Press enter to exit...");
            Console.Read();
        }
    }
}