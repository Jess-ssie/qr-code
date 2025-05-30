using System;
using System.Drawing;
using System.Text;
using QRCoder.Core.Models;

namespace QRCoder.Core.Implementations
{
    public class Renderer 
    {
        public static Image Render(QRCodeMatrix matrix, QRCodeOptions options)
        {
            int pixelSize = options.Scale;
            int quietZone = options.QuietZone;
            int size = matrix.Size + quietZone * 2;

            int imgSize = size * pixelSize;
            Bitmap bmp = new Bitmap(imgSize, imgSize);

            using (Graphics graphics = Graphics.FromImage(bmp))
            {
                graphics.Clear(options.BackgroundColor);
                using (Brush brush = new SolidBrush(options.ForegroundColor))
                {
                    for (int y = 0; y < matrix.Size; y++)
                    {
                        for (int x = 0; x < matrix.Size; x++)
                        {
                            if (matrix[y, x])  // Важно: [row, col] = [y, x]
                            {
                                graphics.FillRectangle(brush,
                                    (x + quietZone) * pixelSize,
                                    (y + quietZone) * pixelSize,
                                    pixelSize,
                                    pixelSize);
                            }
                        }
                    }
                }
            }

            return bmp;
        }

        public static string RenderAsAscii(QRCodeMatrix matrix)
        {
            var sb = new StringBuilder();

            // Верхняя граница
            sb.AppendLine(new string('#', matrix.Size + 2));

            for (int y = 0; y < matrix.Size; y++)
            {
                sb.Append('#');
                for (int x = 0; x < matrix.Size; x++)
                {
                    // Исправлено: matrix[y, x], а не matrix[x, y]
                    sb.Append(matrix[y, x] ? '█' : ' ');
                }
                sb.AppendLine("#");
            }

            // Нижняя граница
            sb.AppendLine(new string('#', matrix.Size + 2));

            return sb.ToString();
        }
    }
}

