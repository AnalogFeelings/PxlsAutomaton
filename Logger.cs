using Pastel;
using System.Drawing;

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
                        Console.WriteLine($"[{"X".Pastel(Color.Red)}] {Message.Pastel(Color.IndianRed)}");
                        break;
                    case LogSeverity.Message:
                        Console.WriteLine($"[{"*".Pastel(Color.CadetBlue)}] {Message.Pastel(Color.LightSkyBlue)}");
                        break;
                    case LogSeverity.Success:
                        Console.WriteLine($"[{"√".Pastel(Color.LightGreen)}] {Message.Pastel(Color.PaleGreen)}");
                        break;
                    case LogSeverity.Warning:
                        Console.WriteLine($"[{"!".Pastel(Color.Gold)}] {Message.Pastel(Color.Yellow)}");
                        break;
                }
            }
        }
    }
}
