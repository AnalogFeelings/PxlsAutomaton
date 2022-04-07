using Spectre.Console;

namespace PxlsAutomaton
{
    public enum LogSeverity
    {
        Message,
        Error,
        Success,
        Warning
    }

    public class Logger
    {
        private static object LoggerLock = new object();

        public static void Log(string Message, LogSeverity Severity)
        {
            lock (LoggerLock)
            {
                switch (Severity)
                {
                    case LogSeverity.Error:
                        AnsiConsole.MarkupLine($"[[[#FF0000]X[/]]] [#CD5C5C]{Message}[/]");
                        break;
                    case LogSeverity.Message:
                        AnsiConsole.MarkupLine($"[[[#5F9EA0]*[/]]] [#87CEFA]{Message}[/]");
                        break;
                    case LogSeverity.Success:
                        AnsiConsole.MarkupLine($"[[[#90EE90]√[/]]] [#98FB98]{Message}[/]");
                        break;
                    case LogSeverity.Warning:
                        AnsiConsole.MarkupLine($"[[[#FFD700]![/]]] [#FFFF00]{Message}[/]");
                        break;
                }
            }
        }
    }
}
