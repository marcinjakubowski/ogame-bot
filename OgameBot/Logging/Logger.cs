using System;

namespace OgameBot.Logging
{
    public class Logger
    {
        public static Logger Instance { get; } = new Logger();
        public LogLevel MinimumLogLevel { get; set; } = LogLevel.Info;
        public bool IncludeTimestamp { get; set; } = false;

        private Logger()
        {

        }

        public void Log(LogLevel level, string message)
        {
            if (level < MinimumLogLevel) return;
            Console.Write("[");

            if (IncludeTimestamp)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write(DateTime.Now);
                Console.ResetColor();
                Console.Write("|");
            }

            switch (level)
            {
                case LogLevel.Debug:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write("DBG");
                    break;
                case LogLevel.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("INF");
                    break;
                case LogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("WRN");
                    break;
                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write("ERR");
                    break;
                case LogLevel.Success:
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.Write("SUC");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }

            Console.ResetColor();
            Console.Write("] ");

            Console.WriteLine(message);
        }

        public void LogException(Exception ex, string message = null)
        {
            Log(LogLevel.Error, $"Exception: {ex.Message}");
            Log(LogLevel.Error, $"Type: {ex.GetType().FullName}");

            if (string.IsNullOrEmpty(message))
                Log(LogLevel.Error, $"Message: {message}");

            Log(LogLevel.Error, ex.StackTrace);
            Log(LogLevel.Error, string.Empty);
        }
    }
}