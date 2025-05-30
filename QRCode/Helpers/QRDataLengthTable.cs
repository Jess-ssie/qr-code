using System;

namespace Helpers
{
    public static class QRDataLengthTable
    {
        public static int GetDataLengthBits(int version, EncodingMode mode)
        {
            // Таблиця довжини даних у бітах залежно від версії та режиму кодування
            int[,] dataLengthBits = new int[3, 4]
            {
                // Numeric, Alphanumeric, Byte, Kanji
                { 10,  9,  8,  8 },  // Версії 1-9
                { 12, 11, 16, 10 },  // Версії 10-26
                { 14, 13, 16, 12 }   // Версії 27-40
            };

            if (version < 1 || version > 40)
                throw new ArgumentException("Version not supported");

            int versionIdx = version <= 9 ? 0 : version <= 26 ? 1 : 2;
            int modeIdx;

            switch (mode)
            {
                case EncodingMode.Numeric:
                    modeIdx = 0;
                    break;
                case EncodingMode.Alphanumeric:
                    modeIdx = 1;
                    break;
                case EncodingMode.Byte:
                    modeIdx = 2;
                    break;
                case EncodingMode.Kanji:
                    modeIdx = 3;
                    break;
                default:
                    throw new ArgumentException("Invalid encoding mode");
            }

            return dataLengthBits[versionIdx, modeIdx];
        }
    }

}
