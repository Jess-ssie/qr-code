using System;
using QRCoder.Core.Models;

namespace QRCoder.Core.Implementations
{
    public class MaskingHandler
    {
        public static QRCodeMatrix ApplyOptimalMaskArray(QRCodeMatrix matrix)
        {
            int bestMask = 0;
            int bestScore = int.MaxValue;
            QRCodeMatrix bestMatrix = null;

            for (int mask = 0; mask < 8; mask++)
            {
                var masked = ApplyMask(matrix.Clone(), mask);
                int score = CalculatePenaltyScore(masked);
                if (score < bestScore)
                {
                    bestScore = score;
                    bestMask = mask;
                    bestMatrix = masked;
                }
            }

            bestMatrix.MaskPattern = bestMask;
            return bestMatrix;
        }

        public static int ApplyOptimalMask(QRCodeMatrix matrix)
        {
            int bestMask = 0;
            int bestScore = int.MaxValue;

            for (int mask = 0; mask < 8; mask++)
            {
                var masked = ApplyMask(matrix.Clone(), mask);
                int score = CalculatePenaltyScore(masked);
                if (score < bestScore)
                {
                    bestScore = score;
                    bestMask = mask;
                }
            }

            return bestMask;
        }


        public static QRCodeMatrix ApplyMask(QRCodeMatrix matrix, int maskPattern)
        {
            int size = matrix.Size;
            var masked = matrix.Clone();

            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size; col++)
                {
                    if (masked.IsReserved(row, col))
                        continue;

                    bool bit = masked[row, col];
                    bool maskBit = GetMaskBit(maskPattern, row, col);

                    if (maskBit)
                    {
                        masked.Invert(row, col);
                    }
                }
            }

            return masked;
        }

        private static bool GetMaskBit(int maskPattern, int row, int col)
        {
            return maskPattern switch
            {
                0 => (row + col) % 2 == 0,
                1 => row % 2 == 0,
                2 => col % 3 == 0,
                3 => (row + col) % 3 == 0,
                4 => ((row / 2) + (col / 3)) % 2 == 0,
                5 => ((row * col) % 2 + (row * col) % 3) == 0,
                6 => (((row * col) % 2 + (row * col) % 3) % 2) == 0,
                7 => (((row + col) % 2 + (row * col) % 3) % 2) == 0,
                _ => false,
            };
        }

        public static int CalculatePenaltyScore(QRCodeMatrix matrix)
        {
            int penalty = 0;
            int size = matrix.Size;

            // Rule 1: Penalty for runs of same color >= 5 (rows)
            for (int row = 0; row < size; row++)
            {
                int runLength = 1;
                bool lastBit = matrix[row, 0];
                for (int col = 1; col < size; col++)
                {
                    if (matrix[row, col] == lastBit)
                    {
                        runLength++;
                        if (runLength == 5) penalty += 3;
                        else if (runLength > 5) penalty++;
                    }
                    else
                    {
                        runLength = 1;
                        lastBit = matrix[row, col];
                    }
                }
            }

            // Rule 1: same for columns
            for (int col = 0; col < size; col++)
            {
                int runLength = 1;
                bool lastBit = matrix[0, col];
                for (int row = 1; row < size; row++)
                {
                    if (matrix[row, col] == lastBit)
                    {
                        runLength++;
                        if (runLength == 5) penalty += 3;
                        else if (runLength > 5) penalty++;
                    }
                    else
                    {
                        runLength = 1;
                        lastBit = matrix[row, col];
                    }
                }
            }

            // Rule 2: 2x2 blocks of same color
            for (int row = 0; row < size - 1; row++)
            {
                for (int col = 0; col < size - 1; col++)
                {
                    bool bit = matrix[row, col];
                    if (bit == matrix[row + 1, col] &&
                        bit == matrix[row, col + 1] &&
                        bit == matrix[row + 1, col + 1])
                    {
                        penalty += 3;
                    }
                }
            }

            // Rule 3: Pattern 1:1:3:1:1 in rows
            for (int row = 0; row < size; row++)
            {
                for (int col = 0; col < size - 6; col++)
                {
                    if (IsPatternAt(matrix, row, col, true))
                        penalty += 40;
                }
            }

            // Rule 3: Pattern 1:1:3:1:1 in columns
            for (int col = 0; col < size; col++)
            {
                for (int row = 0; row < size - 6; row++)
                {
                    if (IsPatternAt(matrix, row, col, false))
                        penalty += 40;
                }
            }

            // Rule 4: Dark module proportion
            int darkModules = 0;
            for (int row = 0; row < size; row++)
                for (int col = 0; col < size; col++)
                    if (matrix[row, col]) darkModules++;

            int totalModules = size * size;
            int percent = (darkModules * 100) / totalModules;
            int fivePercentVariances = Math.Abs(percent - 50) / 5;
            penalty += fivePercentVariances * 10;

            return penalty;
        }

        private static bool IsPatternAt(QRCodeMatrix matrix, int row, int col, bool horizontal)
        {
            bool b(int r, int c) => matrix[r, c];

            if (horizontal)
            {
                if (
                    b(row, col) && !b(row, col + 1) &&
                    b(row, col + 2) && b(row, col + 3) && b(row, col + 4) &&
                    !b(row, col + 5) && b(row, col + 6))
                {
                    // 4 white modules after
                    if (col + 10 < matrix.Size)
                    {
                        for (int i = col + 7; i < col + 11; i++)
                            if (b(row, i)) return false;
                        return true;
                    }
                    // 4 white modules before
                    if (col - 4 >= 0)
                    {
                        for (int i = col - 4; i < col; i++)
                            if (b(row, i)) return false;
                        return true;
                    }
                }
            }
            else
            {
                if (
                    b(row, col) && !b(row + 1, col) &&
                    b(row + 2, col) && b(row + 3, col) && b(row + 4, col) &&
                    !b(row + 5, col) && b(row + 6, col))
                {
                    // 4 white modules below
                    if (row + 10 < matrix.Size)
                    {
                        for (int i = row + 7; i < row + 11; i++)
                            if (b(i, col)) return false;
                        return true;
                    }
                    // 4 white modules above
                    if (row - 4 >= 0)
                    {
                        for (int i = row - 4; i < row; i++)
                            if (b(i, col)) return false;
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
