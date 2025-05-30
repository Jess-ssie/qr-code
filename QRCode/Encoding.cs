
using System.Security.Cryptography;
using Helpers;

namespace Encoding
{
    public class Encoding
    {

        public static byte[] EncodeToByteMode(string data, int version, int maxDataBytes, ErrorCorrectionLevel errorCorrectionLevel)
        {
            if (string.IsNullOrEmpty(data))
            {
                throw new ArgumentException("Дані не можуть бути порожніми.");
            }

            byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(data);

            int[] modeIndicator = GetModeIndicator(EncodingMode.Byte);
            List<int> encodedData = new List<int>(modeIndicator);
            PrintListInConsole(encodedData);
            int lengthBitsCount = QRDataLengthTable.GetDataLengthBits(version, EncodingMode.Byte);
            AddLengthBits(encodedData, dataBytes.Length, lengthBitsCount);

            foreach (byte b in dataBytes)
            {
                for (int i = 7; i >= 0; i--) // Проходимо по 8 бітам байту
                {
                    encodedData.Add((b & (1 << i)) != 0 ? 1 : 0);
                }
            }

            while (encodedData.Count % 8 != 0)
            {
                encodedData.Add(0); // Додаємо 0 до кінця
            }

            // 2. Перевірка на допустиму кількість байтів
            int totalBytes = encodedData.Count / 8; // Перетворюємо біти у байти
            if (totalBytes > maxDataBytes)
            {
                throw new ArgumentException($"Перевищено максимальну кількість байтів ({maxDataBytes}).");
            }

            bool useFirstPaddingByte = true;
            while (encodedData.Count / 8 < maxDataBytes)
            {
                byte paddingByte = useFirstPaddingByte ? (byte)0b11101100 : (byte)0b00010001;
                for (int i = 7; i >= 0; i--) // Додаємо кожен біт байту-заповнювача
                {
                    encodedData.Add((paddingByte & (1 << i)) != 0 ? 1 : 0);
                }
                useFirstPaddingByte = !useFirstPaddingByte;
                totalBytes++;
            }

            Console.WriteLine($"DATA_LENGHT_BEFORE_CORRECTION: {encodedData.Count}");

            List<int> blockSizes = Encoding.CalculateBlockSizes(maxDataBytes, version, errorCorrectionLevel);
            byte[] finalDataBytes = ConvertBitsToBytes(encodedData);
            List<byte[]> distributedBytesOfBlock = DistributeBytesToBlocks(finalDataBytes, blockSizes);

            // Ініціалізація енкодера
            int correctionBytes = QRErrorCorrectionBytes.GetErrorCorrectionBytes(version, errorCorrectionLevel);
            int[] generatorPolynomial = QRErrorCorrectionBytes.GetGeneratorPolynomial(correctionBytes);
            Console.WriteLine($"DEBUG: {generatorPolynomial.Length}, {correctionBytes}");
            ReedSolomonEncoder reedSolomonEncoder = new ReedSolomonEncoder(correctionBytes, generatorPolynomial);

            List<byte[]> errorCorrectionBlocks = new List<byte[]>();

            foreach (var block in distributedBytesOfBlock)
            {
                // Виклик методу для кожного блоку
                // Отримання байтів корекції
                var errorCorrection = reedSolomonEncoder.GenerateErrorCorrectionBytes(block, correctionBytes);
                errorCorrectionBlocks.Add(errorCorrection);
            }


            byte[] finalInterleaved = InterleaveBlocks(distributedBytesOfBlock, errorCorrectionBlocks);
            Console.WriteLine($"DATA_LENGHT_BEFORE_CORRECTION: {finalInterleaved.Length}");
            return finalInterleaved; // або просто поверни byte[]

        }

        private static int[] GetModeIndicator(EncodingMode mode) => mode switch
        {
            EncodingMode.Numeric => new int[] { 0, 0, 0, 1 },
            EncodingMode.Alphanumeric => new int[] { 0, 0, 1, 0 },
            EncodingMode.Byte => new int[] { 0, 1, 0, 0 },
            EncodingMode.Kanji => new int[] { 1, 0, 0, 0 },
            _ => throw new ArgumentException("Unsupported encoding mode")
        };


        public static void PrintListInConsole(List<int> list)
        {
            foreach (int bit in list)
            {
                Console.Write($"{bit}");
            }
            Console.WriteLine();
        }

        public static void AddLengthBits(List<int> encodedData, int dataLength, int bitCount)
        {
            // Проходимо по кожному біту числа від найстаршого до наймолодшого
            for (int i = bitCount - 1; i >= 0; i--)
            {
                // Перевіряємо, чи біт на позиції i дорівнює 1
                int bit = (dataLength & (1 << i)) != 0 ? 1 : 0;

                // Додаємо цей біт у список
                encodedData.Add(bit);
            }
        }

        public static List<int> CalculateBlockSizes(int totalDataBytes, int version, ErrorCorrectionLevel errorCorrectionLevel)
        {
            // Отримуємо кількість блоків для заданої версії та рівня корекції
            int totalBlocks = QRBlockCountTable.GetBlockCount(version, errorCorrectionLevel);

            // Розрахунок розміру кожного блоку
            int baseBlockSize = totalDataBytes / totalBlocks;
            int remainder = totalDataBytes % totalBlocks;

            // Формуємо список розмірів блоків
            List<int> blockSizes = new List<int>();

            for (int i = 0; i < totalBlocks; i++)
            {
                if (i < totalBlocks - remainder)
                {
                    blockSizes.Add(baseBlockSize);
                }
                else
                {
                    blockSizes.Add(baseBlockSize + 1);
                }
            }

            return blockSizes;
        }

        public static List<byte[]> DistributeBytesToBlocks(byte[] dataBytes, List<int> blockSizes)
        {
            List<byte[]> blocks = new List<byte[]>();
            int offset = 0;

            foreach (int blockSize in blockSizes)
            {
                // Формуємо новий блок з відповідним розміром
                byte[] block = new byte[blockSize];
                Array.Copy(dataBytes, offset, block, 0, blockSize);
                blocks.Add(block);

                // Збільшуємо зміщення
                offset += blockSize;
            }

            return blocks;
        }

        public static byte[] ConvertBitsToBytes(List<int> bits)
        {
            if (bits == null || bits.Count == 0)
            {
                throw new ArgumentException("Список бітів не може бути порожнім.");
            }

            // Розраховуємо необхідну кількість байтів
            int byteCount = (bits.Count + 7) / 8; // Додаємо 7, щоб округлити вгору
            byte[] bytes = new byte[byteCount];

            for (int i = 0; i < bits.Count; i++)
            {
                if (bits[i] != 0 && bits[i] != 1)
                {
                    throw new ArgumentException("Список повинен містити лише 0 або 1.");
                }

                // Розраховуємо індекс байта та позицію біта в цьому байті
                int byteIndex = i / 8;
                int bitIndex = 7 - (i % 8); // Починаємо з найстаршого біта

                // Встановлюємо відповідний біт у байті
                bytes[byteIndex] |= (byte)(bits[i] << bitIndex);
            }

            return bytes;
        }

        public static byte[] InterleaveBlocks(List<byte[]> dataBlocks, List<byte[]> ecBlocks)
        {
            if (dataBlocks == null || ecBlocks == null)
                throw new ArgumentNullException("Blocks cannot be null.");

            int totalBlocks = dataBlocks.Count;
            if (ecBlocks.Count != totalBlocks)
                throw new ArgumentException("Кількість блоків даних і блоків корекції має співпадати.");

            // Знаходимо максимальну довжину блоку даних (з урахуванням додаткових байтів)
            int maxDataBlockLength = dataBlocks.Max(b => b.Length);
            int maxEcBlockLength = ecBlocks.Max(b => b.Length);

            List<byte> result = new List<byte>();

            // Спершу чергуємо байти з блоків даних
            for (int i = 0; i < maxDataBlockLength; i++)
            {
                for (int blockIndex = 0; blockIndex < totalBlocks; blockIndex++)
                {
                    byte[] block = dataBlocks[blockIndex];
                    if (i < block.Length)
                    {
                        result.Add(block[i]);
                    }
                    // Якщо байт немає — пропускаємо (як в умові)
                }
            }

            // Потім чергуємо байти з блоків корекції
            for (int i = 0; i < maxEcBlockLength; i++)
            {
                for (int blockIndex = 0; blockIndex < totalBlocks; blockIndex++)
                {
                    byte[] block = ecBlocks[blockIndex];
                    if (i < block.Length)
                    {
                        result.Add(block[i]);
                    }
                }
            }

            return result.ToArray();
        }

    }

}