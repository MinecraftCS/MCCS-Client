using MineCS.rubydung;
using System.Diagnostics;

namespace MineCS
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var processModule = Process.GetCurrentProcess().MainModule;
            if (processModule != null)
            {
                var pathToExe = processModule.FileName;
                var pathToContentRoot = Path.GetDirectoryName(pathToExe);
                Directory.SetCurrentDirectory(pathToContentRoot);
            }

            using (RubyDung game = new RubyDung())
                game.Run();
        }
    }
}