using Microsoft.Build.Framework;
using System;
using System.Xml.Linq;

namespace VsBuild.MSBuildLogger
{
    public class ConsoleWriter
    {
        public static void Write(string message)
        {
            WriteXElement(LoggerElementTypes.Message, message);
        }
        public static void SetColor(ConsoleColor color)
        {
            WriteXElement(LoggerElementTypes.SetColor, color.ToString());
        }
        public static void ResetColor()
        {
            WriteXElement(LoggerElementTypes.ResetColor, string.Empty);
        }
        public static void Error(BuildErrorEventArgs args)
        {
            WriteXElement(LoggerElementTypes.Error, args.Serialize());
        }
        public static void Warning(BuildWarningEventArgs args)
        {
            WriteXElement(LoggerElementTypes.Warning, args.Serialize());
        }
        private static void WriteXElement(LoggerElementTypes type, string data)
        {
            XElement element = new XElement("MSBuildLogger", data.Replace("\r\n", string.Empty));
            element.Add(new XAttribute("Type", type));
            Console.Out.WriteLine(element.ToString());
        }
    }
}
