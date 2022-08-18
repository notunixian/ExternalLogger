using System;
using System.Collections.Generic;
using System.Text;

namespace VRCEXLOGGER
{
    internal static class LoggerUtils
    {
        internal static void Log(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine($"[VRCEX] {message}", ConsoleColor.Red);
          //  Console.Read();
        }
    }
}
