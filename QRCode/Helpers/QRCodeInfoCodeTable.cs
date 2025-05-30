using System;
using Helpers;

public class QRCodeInfoCodeTable
{
    // Таблиця кодів маски та рівня корекції
    private static readonly int[][][] InfoCodes = new int[][][]
    {
        // L
        new int[][]
        {
             new int[]{1,1,1,0,1,1,1,1,1,0,0,0,1,0,0}, // Mask 0
            new int[]{1,1,1,0,0,1,0,1,1,1,1,0,0,1,1}, // Mask 1
            new int[]{1,1,1,1,1,0,1,1,0,1,0,1,0,1,0}, // Mask 2
            new int[]{1,1,1,1,0,0,0,1,0,0,1,1,1,0,1}, // Mask 3
            new int[]{1,1,0,0,1,1,0,0,0,1,0,1,1,1,1}, // Mask 4
            new int[]{1,1,0,0,0,1,1,0,0,0,1,1,0,0,0}, // Mask 5
            new int[]{1,1,0,1,1,0,0,0,1,0,0,0,0,0,1}, // Mask 6
            new int[]{1,1,0,1,0,0,1,0,1,1,1,0,1,1,0}  // Mask 7
        },
        // M
        new int[][]
        {
            new int[]{1,0,1,0,1,0,0,0,0,0,1,0,0,1,0},
            new int[]{1,0,1,0,0,0,1,0,0,1,0,0,1,0,1},
            new int[]{1,0,1,1,1,1,0,0,1,1,1,1,1,0,0},
            new int[]{1,0,1,1,0,1,1,0,0,1,0,0,1,0,1},
            new int[]{1,0,0,0,1,0,1,1,1,1,1,1,1,0,0},
            new int[]{1,0,0,0,0,0,0,1,1,0,0,1,1,1,0},
            new int[]{1,0,0,1,1,1,1,0,0,1,0,1,1,1,1},
            new int[]{1,0,0,1,0,1,0,1,0,0,0,0,0,0,0}
        },
        // Q
        new int[][]
        {
            new int[]{0,1,1,0,1,0,1,0,1,0,1,1,1,1,1},
            new int[]{0,1,1,0,0,0,0,1,1,0,1,0,1,0,0},
            new int[]{0,1,1,1,1,1,1,0,0,1,1,0,0,0,1},
            new int[]{0,1,1,1,0,1,0,0,0,0,0,0,1,1,0},
            new int[]{0,1,0,0,1,0,0,1,0,1,1,0,1,0,0},
            new int[]{0,1,0,0,0,0,1,1,0,0,0,0,1,1,1},
            new int[]{0,1,0,1,1,1,0,1,1,0,1,1,0,1,0},
            new int[]{0,1,0,1,0,1,1,1,1,0,1,1,0,1,1}
        },
        // H
        new int[][]
        {
            new int[] {0,0,1,0,1,1,0,1,0,0,0,1,0,0,1},
            new int[] {0,0,1,0,0,1,1,1,1,1,1,0,1,1,0},
            new int[] {0,0,1,1,1,0,0,1,1,1,0,0,1,1,1},
            new int[] {0,0,1,1,0,0,1,1,1,0,1,0,0,0,0},
            new int[] {0,0,0,0,1,1,1,0,0,1,0,0,1,0,0},
            new int[] {0,0,0,0,0,1,0,1,0,1,0,1,0,1,1},
            new int[] {0,0,0,1,1,0,0,0,0,1,0,0,1,1,0},
            new int[] {0,0,0,1,0,0,0,0,0,1,1,1,0,1,1}
        }
    };

    // Метод для отримання коду інформації
    public static int[] GetInfoCode(ErrorCorrectionLevel errorCorrectionLevel, int maskPattern)
    {
        int rowIndex;
        switch (errorCorrectionLevel)
        {
            case ErrorCorrectionLevel.L:
                rowIndex = 0;
                break;
            case ErrorCorrectionLevel.M:
                rowIndex = 1;
                break;
            case ErrorCorrectionLevel.Q:
                rowIndex = 2;
                break;
            case ErrorCorrectionLevel.H:
                rowIndex = 3;
                break;
            default:
                throw new ArgumentException("Invalid error correction level");
        }

        if (maskPattern < 0 || maskPattern > 7)
        {
            throw new ArgumentException("Mask pattern must be between 0 and 7");
        }

        return InfoCodes[rowIndex][maskPattern];
    }
}
