using System;
using System.Collections.Generic;

namespace Helpers
{
    public static class GeneratorPolynomialGenerator
    {
        // Таблиця логарифмів по основі 2 в полі GF(256)
        private static readonly int[] LogTable = new int[256];
        // Таблиця анtilогарифмів (експонент) у полі GF(256)
        private static readonly int[] AntiLogTable = new int[512]; // 512 для зручності індексації

        static GeneratorPolynomialGenerator()
        {
            InitializeTables();
        }

        private static void InitializeTables()
        {
            int value = 1;
            for (int i = 0; i < 255; i++)
            {
                AntiLogTable[i] = value;
                LogTable[value] = i;
                value = MultiplyNoLUT(value, 2); // базовий примітивний елемент α = 2
            }
            // Дублюємо значення для уникнення модуля при індексації
            for (int i = 255; i < 512; i++)
            {
                AntiLogTable[i] = AntiLogTable[i - 255];
            }
        }

        // Множення без використання таблиць (для ініціалізації)
        private static int MultiplyNoLUT(int a, int b)
        {
            int p = 0;
            for (int i = 0; i < 8; i++)
            {
                if ((b & 1) != 0)
                    p ^= a;
                bool highBitSet = (a & 0x80) != 0;
                a <<= 1;
                if (highBitSet)
                    a ^= 0x11D; // Поліном поля GF(256) для QR-кодів
                b >>= 1;
            }
            return p & 0xFF;
        }

        // Множення у GF(256) з використанням таблиць лог/антілог
        private static int Multiply(int a, int b)
        {
            if (a == 0 || b == 0)
                return 0;
            int logSum = LogTable[a] + LogTable[b];
            return AntiLogTable[logSum];
        }

        // Додавання у GF(256) — це XOR
        private static int Add(int a, int b)
        {
            return a ^ b;
        }

        // Створення генераторного полінома для заданої довжини (кількість байтів корекції)
        public static int[] Generate(int degree)
        {
            if (degree < 1)
                throw new ArgumentException("Degree must be at least 1");

            int[] poly = new int[] { 1 }; // початковий поліном = 1

            for (int i = 0; i < degree; i++)
            {
                poly = MultiplyPolynomials(poly, new int[] { 1, AntiLogTable[i] }); // множимо на (x - α^i)
            }

            return poly;
        }

        // Множення двох поліномів у GF(256)
        private static int[] MultiplyPolynomials(int[] p1, int[] p2)
        {
            int[] result = new int[p1.Length + p2.Length - 1];
            for (int i = 0; i < p1.Length; i++)
            {
                for (int j = 0; j < p2.Length; j++)
                {
                    result[i + j] = Add(result[i + j], Multiply(p1[i], p2[j]));
                }
            }
            return result;
        }
    }
}
