using Figgle;
using Pastel;
using System.Drawing;

namespace PxlsAutomaton
{
    public class MainProgram
    {
        public static void Main(string[] args)
        {
            if (Environment.OSVersion.Version.Major < 10) ConsoleExtensions.Disable();

            Console.Title = "PxlsAutomaton";
            Console.Write(FiggleFonts.Slant.Render("PxlsAutomaton").Pastel("#77b6a9"));
            Console.Write("------------".Pastel(Color.CornflowerBlue));
            Console.Write("By AestheticalZ || https://github.com/AestheticalZ".Pastel(Color.Aqua));
            Console.WriteLine("------------".Pastel(Color.CornflowerBlue));
            Console.WriteLine();

            Bot BotBehavior = new Bot();
            BotBehavior.LoadConfig();
            BotBehavior.InitializeBot().Wait();

            Logger.Log("Press any key to exit...", LogSeverity.Warning);
            Console.ReadKey();
        }
    }
}