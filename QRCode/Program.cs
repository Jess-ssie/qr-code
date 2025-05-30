using System.Drawing;
using Encoding;
using Helpers;
using QRCoder;
using QRCoder.Core.Implementations;
using QRCoder.Core.Models;

namespace Program
{
    class Program
    {
        public static void Main(string[] args)
        {

            var options = new QRCodeOptions
            {
                Version = 5, // авто
                ErrorCorrectionLevel = ErrorCorrectionLevel.H, // Повышенная коррекция ошибок
                EncodingMode = EncodingMode.Byte,
                Scale = 1, // Увеличиваем размер для лучшей читаемости
                QuietZone = 2,
                ForegroundColor = Color.Black,
                BackgroundColor = Color.White
            };


            string inputData = "Hello, QR!";
            int version = 5; // Версія QR-коду
            ErrorCorrectionLevel correctionLevel = ErrorCorrectionLevel.H;
            int capacityByte = QRCapacityVersionTable.GetDataCapacity(version, correctionLevel);
            byte[] encodedData = Encoding.Encoding.EncodeToByteMode(inputData, version, capacityByte, correctionLevel);

            Console.WriteLine("Кодовані дані:");
            foreach (var block in encodedData)
            {
                Console.Write($"{block}-");
            }
            Console.WriteLine();
            QRCodeMatrix matrix = MatrixBuilder.BuildMatrix(encodedData, version, correctionLevel);
            QRCodeMatrix matrixMasked = MaskingHandler.ApplyOptimalMaskArray(matrix);
            Console.WriteLine("YOU DO MATRIXMASK");
            matrixMasked.PrintState();

            Image qrImage = Renderer.Render(matrixMasked, options);

            string outputDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "Output");
            Directory.CreateDirectory(outputDir);  // создаём папку, если её нет
            Console.WriteLine("Ти пройшов створення папки");
            string outputPathPng = Path.Combine(outputDir, "qrcode.png");
            qrImage.Save(outputPathPng, System.Drawing.Imaging.ImageFormat.Png);
            // var placer = new QrMatrixPlacer(version);
            // placer.PrintMatrix();
        }
    }

}


