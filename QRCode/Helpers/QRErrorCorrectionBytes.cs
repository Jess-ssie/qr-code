using System;

namespace Helpers
{
    public static class QRErrorCorrectionBytes
    {
        private static readonly int[,] ErrorCorrectionBytesTable = new int[40, 4]
        {
            // L,   M,   Q,   H
            { 7,   10,   13,   17 }, // Version 1
            { 10,  16,   22,   28 }, // Version 2
            { 15,  26,   36,   44 }, // Version 3
            { 20,  36,   52,   64 }, // Version 4
            { 26,  48,   72,   88 }, // Version 5
            { 36,  64,   96,  112 }, // Version 6
            { 40,  72,  108,  130 }, // Version 7
            { 48,  88,  132,  156 }, // Version 8
            { 60, 110,  160,  192 }, // Version 9
            { 72, 130,  192,  224 }, // Version 10
            { 80, 150,  224,  264 }, // Version 11
            { 96, 176,  260,  308 }, // Version 12
            { 104,198,  288,  352 }, // Version 13
            { 120,216,  320,  384 }, // Version 14
            { 132,240,  360,  432 }, // Version 15
            { 144,280,  408,  480 }, // Version 16
            { 168,308,  448,  532 }, // Version 17
            { 180,338,  504,  588 }, // Version 18
            { 196,364,  546,  644 }, // Version 19
            { 224,416,  600,  700 }, // Version 20
            { 224,442,  644,  756 }, // Version 21
            { 252,476,  690,  816 }, // Version 22
            { 270,504,  750,  900 }, // Version 23
            { 300,560,  810,  960 }, // Version 24
            { 312,588,  870, 1050 }, // Version 25
            { 336,644,  952, 1110 }, // Version 26
            { 360,700, 1020, 1200 }, // Version 27
            { 390,728, 1050, 1260 }, // Version 28
            { 420,784, 1140, 1350 }, // Version 29
            { 450,812, 1200, 1440 }, // Version 30
            { 480,868, 1290, 1530 }, // Version 31
            { 510,924, 1350, 1620 }, // Version 32
            { 540,980, 1440, 1710 }, // Version 33
            { 570,1036,1530, 1800 }, // Version 34
            { 570,1064,1590, 1890 }, // Version 35
            { 600,1120,1680, 1980 }, // Version 36
            { 630,1204,1770, 2100 }, // Version 37
            { 660,1260,1860, 2220 }, // Version 38
            { 720,1316,1980, 2340 }, // Version 39
            { 750,1372,2070, 2460 }  // Version 40
        };

        public static int GetErrorCorrectionBytes(int version, ErrorCorrectionLevel level)
        {
            if (version < 1 || version > 40)
                throw new ArgumentOutOfRangeException("Version must be between 1 and 40.");

            int levelIndex = level switch
            {
                ErrorCorrectionLevel.L => 0,
                ErrorCorrectionLevel.M => 1,
                ErrorCorrectionLevel.Q => 2,
                ErrorCorrectionLevel.H => 3,
                _ => throw new ArgumentException("Invalid error correction level")
            };

            return ErrorCorrectionBytesTable[version - 1, levelIndex];
        }

        public static int[] GetGeneratorPolynomial(int errorCorrectionBytes)
        {
            int[] generatorPolynomials = GeneratorPolynomialGenerator.Generate(errorCorrectionBytes);

            return generatorPolynomials;
        }
    }
}
