using System;
using System.Drawing;

namespace VsBuild.VsExtension
{
    public static class ColorExtensions
    {
        public static System.Windows.Media.Color MediaColor(this ConsoleColor color)
        {
            Color drawingColor = color.DrawingColor();
            return System.Windows.Media.Color.FromArgb(drawingColor.A, drawingColor.R, drawingColor.G, drawingColor.B);
        }

        public static Color DrawingColor(this ConsoleColor color)
        {
            Color drawingColor;
            switch (color)
            {
                case ConsoleColor.Black:
                    drawingColor = Color.Black;
                    break;

                case ConsoleColor.Blue:
                    drawingColor = Color.Blue;
                    break;

                case ConsoleColor.Cyan:
                    drawingColor = Color.Cyan;
                    break;

                case ConsoleColor.DarkBlue:
                    drawingColor = ColorTranslator.FromHtml("#000080");
                    break;

                case ConsoleColor.DarkGray:
                    drawingColor = ColorTranslator.FromHtml("#808080");
                    break;

                case ConsoleColor.DarkGreen:
                    drawingColor = ColorTranslator.FromHtml("#008000");
                    break;

                case ConsoleColor.DarkMagenta:
                    drawingColor = ColorTranslator.FromHtml("#800080");
                    break;

                case ConsoleColor.DarkRed:
                    drawingColor = ColorTranslator.FromHtml("#800000");
                    break;

                case ConsoleColor.DarkYellow:
                    drawingColor = ColorTranslator.FromHtml("#808000");
                    break;

                case ConsoleColor.Gray:
                    drawingColor = ColorTranslator.FromHtml("#C0C0C0");
                    break;

                case ConsoleColor.Green:
                    drawingColor = ColorTranslator.FromHtml("#00FF00");
                    break;

                case ConsoleColor.Magenta:
                    drawingColor = Color.Magenta;
                    break;

                case ConsoleColor.Red:
                    drawingColor = Color.Red;
                    break;

                case ConsoleColor.White:
                    drawingColor = Color.White;
                    break;

                default:
                    drawingColor = Color.Yellow;
                    break;
            }
            return drawingColor;
        }
    }
}
