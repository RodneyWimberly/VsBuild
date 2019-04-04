using Microsoft.Build.Framework;
using Newtonsoft.Json;
using System;

namespace VsBuild.MSBuildLogger
{
    public class ConsoleWriter
    {
        public static void Write(string message)
        {
            Console.Out.Write(message);
        }
        public static void SetColor(ConsoleColor color)
        {
            Console.Out.Write($"#SetColor:{color}#");
        }
        public static void ResetColor()
        {
            Console.Out.Write("#ResetColor#");
        }
        public static void Error(BuildErrorEventArgs args)
        {
            Console.Out.Write($"#BuildError:{JsonConvert.SerializeObject(args)}#");
        }
        public static void Warning(BuildWarningEventArgs args)
        {
            Console.Out.Write($"#BuildWarning:{JsonConvert.SerializeObject(args)}#");
        }
    }
}
