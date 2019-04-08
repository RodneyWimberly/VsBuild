using System;
using System.Drawing;
using MediaColor = System.Windows.Media.Color;
using DrawingColor = System.Drawing.Color;

namespace VsBuild.VsExtension
{
    public static class ColorExtensions
    {
        public static string ToHtmlColor(this ConsoleColor consoleColor)
        {
            return ColorTranslator.ToHtml(consoleColor.ToDrawingColor());
        }

        public static string ToHtmlColor(this MediaColor mediaColor)
        {
            return ColorTranslator.ToHtml(mediaColor.ToDrawingColor());
        }

        public static string ToHtmlColor(this DrawingColor drawingColor)
        {
            return ColorTranslator.ToHtml(drawingColor);
        }

        public static MediaColor ToMediaColor(this string htmlColor)
        {
            return ColorTranslator.FromHtml(htmlColor).ToMediaColor();
        }

        public static MediaColor ToMediaColor(this DrawingColor drawingColor)
        {
            return MediaColor.FromArgb(drawingColor.A, drawingColor.R, drawingColor.G, drawingColor.B);
        }

        public static MediaColor ToMediaColor(this ConsoleColor color)
        {
            return color.ToDrawingColor().ToMediaColor();
        }

        public static DrawingColor ToDrawingColor(this string htmlColor)
        {
            return ColorTranslator.FromHtml(htmlColor);
        }

        public static DrawingColor ToDrawingColor(this MediaColor mediaColor)
        {
            return DrawingColor.FromArgb(mediaColor.A, mediaColor.R, mediaColor.G, mediaColor.B);
        }

        public static DrawingColor ToDrawingColor(this ConsoleColor color)
        {
            DrawingColor drawingColor;
            switch (color)
            {
                case ConsoleColor.Black:
                    drawingColor = DrawingColor.Black;
                    break;

                case ConsoleColor.Blue:
                    drawingColor = DrawingColor.Blue;
                    break;

                case ConsoleColor.Cyan:
                    drawingColor = DrawingColor.Cyan;
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
                    drawingColor = DrawingColor.Magenta;
                    break;

                case ConsoleColor.Red:
                    drawingColor = DrawingColor.Red;
                    break;

                case ConsoleColor.White:
                    drawingColor = DrawingColor.White;
                    break;

                default:
                    drawingColor = DrawingColor.Yellow;
                    break;
            }
            return drawingColor;
        }
    }
}
