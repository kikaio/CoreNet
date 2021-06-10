using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CoreNet.Utils.Loggers
{
    public class ConsoleLogger : CoreLogger
    {
        public ConsoleLogger() : base()
        {

        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public override void Write(string _log)
        {
            Console.WriteLine($"[{DateTime.UtcNow.ToString("yy/MM/dd HH:mm:ss"),-17}]{_log}");
        }


        public override void WriteTrace(string _log, [CallerMemberName] string _mName = "", [CallerLineNumber] int _line = 0)
        {
            string header = $"{DateTime.UtcNow.ToString("yy/MM/dd HH:mm:ss"),-17}::{_mName}::{_line,3}";
            Console.WriteLine($"[{header}]{_log}");
        }

        public override void WriteDebug(string _log)
        {
            ConsoleColor preColor = Console.ForegroundColor;
            if (Console.ForegroundColor != ConsoleColor.Yellow)
                Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[{DateTime.UtcNow.ToString("yy/MM/dd HH:mm:ss"),-17}]{_log}");
            Console.ForegroundColor = preColor;
        }

        public override void WriteDebugTrace(string _log = "", [CallerMemberName] string _mName = "", [CallerLineNumber] int _line = 0)
        {
            ConsoleColor preColor = Console.ForegroundColor;
            if (Console.ForegroundColor != ConsoleColor.Yellow)
                Console.ForegroundColor = ConsoleColor.Yellow;
            string header = $"{DateTime.UtcNow.ToString("yy/MM/dd HH:mm:ss"),-17}::{_mName}::{_line,3}";
            Console.WriteLine($"[{header}]{_log}");
            Console.ForegroundColor = preColor;
        }

        public override void WriteDebugWarn(string _log)
        {
            ConsoleColor preColor = Console.ForegroundColor;
            if (Console.ForegroundColor != ConsoleColor.Magenta)
                Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"[{DateTime.UtcNow.ToString("yy/MM/dd HH:mm:ss"),-17}]{_log}");
            Console.ForegroundColor = preColor;
        }

        public override void WriteDebugError(string _log)
        {
            ConsoleColor preColor = Console.ForegroundColor;
            if (Console.ForegroundColor != ConsoleColor.DarkRed)
                Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"[{DateTime.UtcNow.ToString("yy/MM/dd HH:mm:ss"),-17}]{_log}");
            Console.ForegroundColor = preColor;
        }

        public override void Error(string _log)
        {
            ConsoleColor preColor = Console.ForegroundColor;
            if (Console.ForegroundColor != ConsoleColor.Red)
                Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[{DateTime.UtcNow.ToString("yy/MM/dd HH:mm:ss"),-17}]{_log}");
            Console.ForegroundColor = preColor;
        }
    }

}
