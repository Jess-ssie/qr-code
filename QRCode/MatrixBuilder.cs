using System;
using Helpers;
using QRCoder.Core.Models;

namespace QRCoder.Core.Implementations
{
    public class MatrixBuilder
    {
        public static QRCodeMatrix BuildMatrix(byte[] data, int version, ErrorCorrectionLevel errorCorrectionLevel)
        {
            if (version < 1 || version > 40)
                throw new ArgumentException("Version not supported");


            int size = 21 + (version - 1) * 4;
            var matrix = new QRCodeMatrix(size);


            // 1. Размещаем Finder Patterns (по углам)
            Console.WriteLine("INITIAL:");
            matrix.PrintState();
            Console.WriteLine("_________________________");

            PlaceFinderPatterns(matrix);
            Console.WriteLine("1. _________________________");
            matrix.PrintState();
            Console.WriteLine();
            Console.WriteLine();
            matrix.PrintStateComplicated();
            Console.WriteLine("_________________________");

            // 2. Для версии >= 2 добавляем Alignment Patterns
            if (version >= 2)
                PlaceAlignmentPatterns(matrix, version);
            Console.WriteLine("2. _________________________");
            matrix.PrintState();
            Console.WriteLine("_________________________");
            // 3. Добавляем Timing Patterns
            PlaceTimingPatterns(matrix);
            Console.WriteLine("3. _________________________");
            matrix.PrintState();
            Console.WriteLine();
            Console.WriteLine();
            matrix.PrintStateComplicated();
            Console.WriteLine("_________________________");
            // 4. Добавляем Dark Module
            PlaceDarkModule(matrix);
            Console.WriteLine("4. _________________________");
            matrix.PrintState();
            Console.WriteLine("_________________________");

            int bestMask = MaskingHandler.ApplyOptimalMask(matrix);
            int[] formatBits = QRCodeInfoCodeTable.GetInfoCode(errorCorrectionLevel, bestMask);
            for (int i = 0; i < 15; i++)
            {
                // Розміщення по рядках і стовпцях
                PlaceSingleFormatBit(matrix, i, formatBits[i] == 1);
            }
            matrix.PrintState();
            Console.WriteLine();
            Console.WriteLine();
            matrix.PrintStateComplicated();

            // 5. Розміщуємо модулі Версії
            InsertVersionInfo(matrix, version);
            Console.WriteLine("5. _________________________");
            matrix.PrintState();
            Console.WriteLine("_________________________");

            Console.WriteLine($"DATA: {data.Length}");
            string separator = " ";
            foreach (byte b in data)
            {
                // Convert.ToString(b, 2) перетворює байт на двійковий рядок.
                // PadLeft(8, '0') додає ведучі нулі, щоб рядок завжди мав довжину 8 символів.
                string binaryString = Convert.ToString(b, 2).PadLeft(8, '0');
                Console.Write($"{binaryString}{separator}");
            }
            Console.WriteLine();
            // 6. Размещаем данные
            PlaceData(matrix, data);
            Console.WriteLine("6. _________________________");
            matrix.PrintState();
            Console.WriteLine("_________________________");
            Console.WriteLine($"MATRIX_SIZE: {matrix.Size}");
            return matrix;
        }

        private static void PlaceFinderPatterns(QRCodeMatrix matrix)
        {
            int size = matrix.Size;
            PlaceFinderPattern(matrix, 0, 0);             // Верхний левый угол
            for (int i = 0; i < 8; i++)
            {
                if (i == 7)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        matrix.SetModule(i, j, false, isFunction: true);
                    }
                    continue;
                }
                matrix.SetModule(i, 7, false, isFunction: true);
            }
            // matrix.SetModule(0, 7, false, isFunction: true);
            PlaceFinderPattern(matrix, 0, size - 7);      // Верхний правый угол
            for (int i = 0; i < 8; i++)
            {
                if (i == 7)
                {
                    for (int j = size - 8; j < size; j++)
                    {
                        matrix.SetModule(i, j, false, isFunction: true);
                    }
                    continue;
                }
                matrix.SetModule(i, size - 8, false, isFunction: true);
            }
            PlaceFinderPattern(matrix, size - 7, 0);      // Нижний левый угол
            for (int i = size - 8; i < size; i++)
            {
                if (i == size - 8)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        matrix.SetModule(i, j, false, isFunction: true);
                    }
                    continue;
                }
                matrix.SetModule(i, 7, false, isFunction: true);
            }
        }

        private static void PlaceFinderPattern(QRCodeMatrix matrix, int row, int col)
        {
            int[,] pattern = {
                {1,1,1,1,1,1,1},
                {1,0,0,0,0,0,1},
                {1,0,1,1,1,0,1},
                {1,0,1,1,1,0,1},
                {1,0,1,1,1,0,1},
                {1,0,0,0,0,0,1},
                {1,1,1,1,1,1,1},
            };
            for (int dy = 0; dy < 7; dy++)
                for (int dx = 0; dx < 7; dx++)
                    matrix.SetModule(row + dy, col + dx, pattern[dy, dx] == 1, isFunction: true);
        }

        private static void PlaceAlignmentPatterns(QRCodeMatrix matrix, int version)
        {
            if (version == 1)
                return; // Нет alignment patterns для версии 1

            int[] positions = GetAlignmentPatternPositions(version);

            foreach (int row in positions)
            {
                foreach (int col in positions)
                {
                    if (!matrix.IsReserved(row, col))
                    {
                        PlaceAlignmentPattern(matrix, row - 2, col - 2);
                    }
                }
            }
        }
        private static readonly int[][] AlignmentPatternPositionsTable = new int[][]
        {
            new int[0],                     // Version 1 — нет alignment паттернов
            new int[] {6, 18},             // Version 2
            new int[] {6, 22},             // Version 3
            new int[] {6, 26},             // Version 4
            new int[] {6, 30},             // Version 5
            new int[] {6, 34},             // Version 6
            new int[] {6, 22, 38},         // Version 7
            new int[] {6, 24, 42},         // Version 8
            new int[] {6, 26, 46},         // Version 9
            new int[] {6, 28, 50},         // Version 10
            new int[] {6, 30, 54},         // Version 11
            new int[] {6, 32, 58},         // Version 12
            new int[] {6, 34, 62},         // Version 13
            new int[] {6, 26, 46, 66},     // Version 14
            new int[] {6, 26, 48, 70},     // Version 15
            new int[] {6, 26, 50, 74},     // Version 16
            new int[] {6, 30, 54, 78},     // Version 17
            new int[] {6, 30, 56, 82},     // Version 18
            new int[] {6, 30, 58, 86},     // Version 19
            new int[] {6, 34, 62, 90},     // Version 20
            new int[] {6, 28, 50, 72, 94}, // Version 21
            new int[] {6, 26, 50, 74, 98}, // Version 22
            new int[] {6, 30, 54, 78, 102},// Version 23
            new int[] {6, 28, 54, 80, 106},// Version 24
            new int[] {6, 32, 58, 84, 110},// Version 25
            new int[] {6, 30, 58, 86, 114},// Version 26
            new int[] {6, 34, 62, 90, 118},// Version 27
            new int[] {6, 30, 54, 78, 102, 126}, // Version 28
            new int[] {6, 24, 50, 76, 102, 128}, // Version 29
            new int[] {6, 28, 54, 80, 106, 132}, // Version 30
            new int[] {6, 32, 58, 84, 110, 136}, // Version 31
            new int[] {6, 26, 54, 82, 110, 138}, // Version 32
            new int[] {6, 30, 58, 86, 114, 142}, // Version 33
            new int[] {6, 34, 62, 90, 118, 146}, // Version 34
            new int[] {6, 30, 54, 78, 102, 126, 150}, // Version 35
            new int[] {6, 24, 50, 76, 102, 128, 154}, // Version 36
            new int[] {6, 28, 54, 80, 106, 132, 158}, // Version 37
            new int[] {6, 32, 58, 84, 110, 136, 162}, // Version 38
            new int[] {6, 26, 54, 82, 110, 138, 166}, // Version 39
            new int[] {6, 30, 58, 86, 114, 142, 170}  // Version 40
        };

        private static int[] GetAlignmentPatternPositions(int version)
        {
            if (version < 1 || version > 40)
                throw new ArgumentOutOfRangeException(nameof(version), "Version must be between 1 and 40.");

            return AlignmentPatternPositionsTable[version - 1];
        }

        private static void PlaceAlignmentPattern(QRCodeMatrix matrix, int row, int col)
        {
            int[,] pattern = {
                {1,1,1,1,1},
                {1,0,0,0,1},
                {1,0,1,0,1},
                {1,0,0,0,1},
                {1,1,1,1,1},
            };
            for (int dy = 0; dy < 5; dy++)
                for (int dx = 0; dx < 5; dx++)
                    matrix.SetModule(row + dy, col + dx, pattern[dy, dx] == 1, isFunction: true);
        }

        private static void PlaceTimingPatterns(QRCodeMatrix matrix)
        {
            int size = matrix.Size;
            for (int i = 8; i < size - 8; i++)
            {
                bool value = (i % 2) == 0;
                matrix.SetModule(6, i, value, isFunction: true);
                matrix.SetModule(i, 6, value, isFunction: true);
            }
        }

        private static void PlaceDarkModule(QRCodeMatrix matrix)
        {
            int row = matrix.Size - 8;
            int col = 8;
            matrix.SetModule(row, col, true, isFunction: true);
        }

        private static void PlaceSingleFormatBit(QRCodeMatrix matrix, int index, bool bit)
        {
            int size = matrix.Size;

            // Розміщення форматних бітів у стандартних позиціях
            if (index < 7)
            {
                if (matrix.IsReserved(8, index))
                {
                    matrix.SetModule(8, index + 1, bit, true);
                    matrix.SetModule(size - 1 - index, 8, bit, true);
                    return;
                }
                // Ліва верхня горизонтальна позиція
                matrix.SetModule(8, index, bit, true);
                // Ліва нижня вертикальна позиція
                matrix.SetModule(size - 1 - index, 8, bit, true);
            }
            else
            if (index < 9)
            {
                // Ліва вертикальна позиція
                matrix.SetModule(8 - (index - 7), 8, bit, true);
                // Права Верхня горизонтальна позиція
                matrix.SetModule(8, size - 8 + (index - 7), bit, true);
            }
            else
            {
                // Ліва вертикальна позиція
                matrix.SetModule(8 - (index - 7) - 1, 8, bit, true);
                // Права Верхня горизонтальна позиція
                matrix.SetModule(8, size - 8 + (index - 7), bit, true);
            }
        }

        public static void InsertVersionInfo(QRCodeMatrix matrix, int version)
        {
            // Версионная информация нужна только для QR-кодов версии 7 и выше
            if (version < 7) return;

            // Вычисляем 18 бит version info (6 бит версии + 12 бит BCH-кода)
            int versionBits = version << 12;
            int poly = 0x1f25; // BCH-полином для version info
            int bch = versionBits;
            for (int i = 0; i < 6; i++)
            {
                if (((bch >> (17 - i)) & 1) == 1)
                {
                    bch ^= poly << (5 - i);
                }
            }
            int versionInfo = (version << 12) | (bch & 0xFFF);
            Console.WriteLine($"VERSIONINFO: {versionInfo}");
            // Вставляем в два блока 3x6
            int size = matrix.Size;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    bool bit = ((versionInfo >> (1 + j * 3 + i)) & 1) == 1;
                    // Зверху 
                    matrix.SetModule(j, size - 11 + i, bit, isFunction: true);
                    // Зліва
                    matrix.SetModule(size - 11 + i, j, bit, isFunction: true);
                }
            }
        }

        private static void PlaceData(QRCodeMatrix matrix, byte[] data)
        {
            int size = matrix.Size;
            int bitIndex = 0;
            int totalBits = data.Length * 8;
            int direction = -1;
            int col = size - 1;
            int bitsUsed = 0;
            while (col > 0)
            {
                // if (col == 6) col--; // пропускаем timing pattern
                int row = (direction == -1) ? size - 1 : 0;
                int rowEnd = (direction == -1) ? -1 : size;
                for (; row != rowEnd; row += direction)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        int c = col - i;
                        if (matrix.IsReserved(row, c)) continue;
                        bool bit = false;
                        if (bitIndex < totalBits)
                        {
                            int byteIndex = bitIndex / 8;
                            int bitInByte = 7 - (bitIndex % 8);
                            bit = (data[byteIndex] & (1 << bitInByte)) != 0;
                        }
                        matrix.SetModule(row, c, bit, isFunction: false);
                        bitIndex++;
                        bitsUsed++;
                    }
                }
                direction = -direction;
                col -= 2;
            }
            Console.WriteLine($"TOTAL_BITS: {totalBits}");
            Console.WriteLine($"USED_BITS: {bitsUsed}");
        }
    }
}
