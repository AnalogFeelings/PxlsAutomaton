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
                        Console.Write($"[{"X".Pastel(Color.Red)}] {Message.Pastel(Color.IndianRed)}");
                        break;
                    case LogSeverity.Message:
                        Console.Write($"[{"*".Pastel(Color.CadetBlue)}] {Message.Pastel(Color.LightSkyBlue)}");
                        break;
                    case LogSeverity.Success:
                        Console.Write($"[{"√".Pastel(Color.LightGreen)}] {Message.Pastel(Color.PaleGreen)}");
                        break;
                    case LogSeverity.Warning:
                        Console.Write($"[{"!".Pastel(Color.Gold)}] {Message.Pastel(Color.Yellow)}");
                        break;
                }
            }
        }
    }
}
