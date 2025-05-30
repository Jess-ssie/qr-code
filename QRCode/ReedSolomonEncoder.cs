using System;
using System.Collections.Generic;

namespace Encoding
{
    public class ReedSolomonEncoder
    {
        private readonly int[] generatorPolynomial;

        public ReedSolomonEncoder(int correctionBytes, int[] generatorPolynomial)
        {
            if (correctionBytes <= 0 || generatorPolynomial == null )
            {
                throw new ArgumentException("Invalid generator polynomial or correction byte count.");
            }

            this.generatorPolynomial = generatorPolynomial;
        }

        public byte[] GenerateErrorCorrectionBytes(byte[] data, int correctionBytes)
        {
            if (data == null || data.Length == 0 || correctionBytes <= 0)
            {
                throw new ArgumentException("Invalid data or correction byte count.");
            }

            // Create the prepared array and initialize with data and trailing zeros
            byte[] preparedArray = new byte[data.Length + correctionBytes];
            Array.Copy(data, preparedArray, data.Length);

            // Perform Reed-Solomon encoding
            for (int i = 0; i < data.Length; i++)
            {
                byte firstElement = preparedArray[0];
                Array.Copy(preparedArray, 1, preparedArray, 0, preparedArray.Length - 1);
                preparedArray[^1] = 0; // Fill last element with zero

                if (firstElement == 0)
                {
                    continue; // Skip if the first element is 0
                }

                int alphaIndex = GaloisField.GetAlphaIndex(firstElement);
                for (int j = 0; j < correctionBytes; j++)
                {
                    int value = GaloisField.Add(alphaIndex, generatorPolynomial[j]);
                    preparedArray[j] ^= GaloisField.GetValue(value);
                }
            }

            // Return the last `correctionBytes` as error correction codes
            byte[] errorCorrectionBytes = new byte[correctionBytes];
            Array.Copy(preparedArray, 0, errorCorrectionBytes, 0, correctionBytes);

            return errorCorrectionBytes;
        }
    }

    public static class GaloisField
    {
        private static readonly int[] Exponents = new int[256];
        private static readonly int[] Logarithms = new int[256];
        private const int PrimitivePolynomial = 0x11D;

        static GaloisField()
        {
            int value = 1;
            for (int i = 0; i < 256; i++)
            {
                Exponents[i] = value;
                Logarithms[value] = i;
                value <<= 1;
                if (value >= 256)
                {
                    value ^= PrimitivePolynomial;
                }
            }
        }

        public static int GetAlphaIndex(byte value)
        {
            return Logarithms[value];
        }

        public static byte GetValue(int alphaIndex)
        {
            return (byte)Exponents[alphaIndex % 255];
        }

        public static int Add(int alphaIndex1, int alphaIndex2)
        {
            return (alphaIndex1 + alphaIndex2) % 255;
        }
    }
}
