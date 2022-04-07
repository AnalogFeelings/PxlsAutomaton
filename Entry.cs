using Figgle;
using Spectre.Console;

namespace PxlsAutomaton
{
    public class MainProgram
    {
        public static void Main(string[] args)
        {
            Console.Title = "PxlsAutomaton";
            Console.WindowWidth = 160;
            AnsiConsole.Markup($"[#77b6a9]{FiggleFonts.Slant.Render("PxlsAutomaton")}[/]");
            AnsiConsole.MarkupLine("[#6495ed]===========[/] [#00ffff]By AestheticalZ || https://github.com/AestheticalZ[/] [#6495ed]===========[/]");
            AnsiConsole.WriteLine();

            Bot BotBehavior = new Bot();
            BotBehavior.LoadConfig();
            BotBehavior.InitializeBot().Wait();

            Logger.Log("Press any key to exit...", LogSeverity.Warning);
            Console.ReadKey();
        }
    }
}