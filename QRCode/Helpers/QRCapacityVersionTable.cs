using System;

namespace Helpers
{
    public static class QRCapacityVersionTable
    {
        public static int GetDataCapacity(int version, ErrorCorrectionLevel level)
        {
            // Полная таблица вместимости (количество байт) для версий 1-40 и всех уровней коррекции
            // Данные взяты из стандарта ISO/IEC 18004 и исходников QRCoder
            int[,] capacities = new int[40, 4]
            {
                // L,   M,   Q,   H
                {  19,  16,  13,   9 }, // 1
                {  34,  28,  22,  16 }, // 2
                {  55,  44,  34,  26 }, // 3
                {  80,  64,  48,  36 }, // 4
                { 108,  86,  62,  46 }, // 5
                { 136, 108,  76,  60 }, // 6
                { 156, 124,  88,  66 }, // 7
                { 194, 154, 110,  86 }, // 8
                { 232, 182, 132, 100 }, // 9
                { 274, 216, 154, 122 }, // 10
                { 324, 254, 180, 140 }, // 11
                { 370, 290, 206, 158 }, // 12
                { 428, 334, 244, 180 }, // 13
                { 461, 365, 261, 197 }, // 14
                { 523, 415, 295, 223 }, // 15
                { 589, 453, 325, 253 }, // 16
                { 647, 507, 367, 283 }, // 17
                { 721, 563, 397, 313 }, // 18
                { 795, 627, 445, 341 }, // 19
                { 861, 669, 485, 385 }, // 20
                { 932, 714, 512, 406 }, // 21
                {1006, 782, 568, 442 }, // 22
                {1094, 860, 614, 464 }, // 23
                {1174, 914, 664, 514 }, // 24
                {1276,1000, 718, 538 }, // 25
                {1370,1062, 754, 596 }, // 26
                {1468,1128, 808, 628 }, // 27
                {1531,1193, 871, 661 }, // 28
                {1631,1267, 911, 701 }, // 29
                {1735,1373, 985, 745 }, // 30
                {1843,1455,1033, 793 }, // 31
                {1955,1541,1115, 845 }, // 32
                {2071,1631,1171, 901 }, // 33
                {2191,1725,1231, 961 }, // 34
                {2306,1812,1286, 986 }, // 35
                {2434,1914,1354,1054 }, // 36
                {2566,1992,1426,1096 }, // 37
                {2702,2102,1502,1142 }, // 38
                {2812,2216,1582,1222 }, // 39
                {2956,2334,1666,1276 }  // 40
            };

            if (version < 1 || version > 40)
                throw new ArgumentException("Version not supported");

            int levelIdx;
            switch (level)
            {
                case ErrorCorrectionLevel.L:
                    levelIdx = 0;
                    break;
                case ErrorCorrectionLevel.M:
                    levelIdx = 1;
                    break;
                case ErrorCorrectionLevel.Q:
                    levelIdx = 2;
                    break;
                case ErrorCorrectionLevel.H:
                    levelIdx = 3;
                    break;
                default:
                    throw new ArgumentException("Invalid error correction level");
            }

            return capacities[version - 1, levelIdx];
        }
    }
}