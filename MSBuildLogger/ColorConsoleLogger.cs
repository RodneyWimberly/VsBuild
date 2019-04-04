using Microsoft.Build.Framework;
using Microsoft.Build.Logging;

namespace VsBuild.MSBuildLogger
{
    public class ColorConsoleLogger : ConsoleLogger
    {
        public ColorConsoleLogger() 
            : this(LoggerVerbosity.Normal) { }

        public ColorConsoleLogger(LoggerVerbosity verbosity) 
            : this(verbosity, ConsoleWriter.Write, ConsoleWriter.SetColor, ConsoleWriter.ResetColor) { }

        public ColorConsoleLogger(LoggerVerbosity verbosity, WriteHandler write, ColorSetter colorSet, ColorResetter colorReset) 
            : base(verbosity, write, colorSet, colorReset) { }

        public new void ErrorHandler(object sender, BuildErrorEventArgs e)
        {
            ConsoleWriter.Error(e);
            base.ErrorHandler(sender, e);
        }

        public new void WarningHandler(object sender, BuildWarningEventArgs e)
        {
            ConsoleWriter.Warning(e);
            base.WarningHandler(sender, e);
        }
    }
}
