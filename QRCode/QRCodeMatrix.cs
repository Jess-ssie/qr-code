using System;

namespace QRCoder.Core.Models
{
    public class QRCodeMatrix
    {
        private readonly bool[,] _matrix;
        private readonly bool[,] _reserved;

        public int Size { get; }
        public int MaskPattern { get; set; }

        public QRCodeMatrix(int size)
        {
            Size = size;
            _matrix = new bool[Size, Size];
            _reserved = new bool[Size, Size];
        }

        public bool this[int row, int col]
        {
            get
            {
                if (row < 0 || row >= Size || col < 0 || col >= Size)
                    return false;
                return _matrix[row, col];
            }
            set
            {
                if (row < 0 || row >= Size || col < 0 || col >= Size)
                    return;

                if (!_reserved[row, col])
                {
                    _matrix[row, col] = value;
                }
            }
        }

        public bool IsReserved(int row, int col)
        {
            if (row < 0 || row >= Size || col < 0 || col >= Size)
                return false;

            return _reserved[row, col];
        }

        public void Reserve(int row, int col)
        {
            if (row < 0 || row >= Size || col < 0 || col >= Size)
                return;

            _reserved[row, col] = true;
        }

        public void ReserveArea(int startRow, int startCol, int width, int height)
        {
            for (int row = startRow; row < startRow + height; row++)
            {
                for (int col = startCol; col < startCol + width; col++)
                {
                    if (row >= 0 && row < Size && col >= 0 && col < Size)
                    {
                        _reserved[row, col] = true;
                    }
                }
            }
        }

        public void Invert(int row, int col)
        {
            if (row < 0 || row >= Size || col < 0 || col >= Size)
                return;

            if (!_reserved[row, col])
            {
                _matrix[row, col] = !_matrix[row, col];
            }
        }

        public void Clear()
        {
            Array.Clear(_matrix, 0, _matrix.Length);
            Array.Clear(_reserved, 0, _reserved.Length);
        }

        public QRCodeMatrix Clone()
        {
            var clone = new QRCodeMatrix(Size)
            {
                MaskPattern = this.MaskPattern
            };

            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    clone._matrix[row, col] = this._matrix[row, col];
                    clone._reserved[row, col] = this._reserved[row, col];
                }
            }

            return clone;
        }

        public void SetModule(int row, int col, bool value, bool isFunction = false)
        {
            if (row < 0 || row >= Size || col < 0 || col >= Size)
                return;

            _matrix[row, col] = value;

            if (isFunction)
            {
                _reserved[row, col] = true;
            }
        }

        public bool IsFunctionModule(int row, int col)
        {
            if (row < 0 || row >= Size || col < 0 || col >= Size)
                return false;

            return _reserved[row, col];
        }

        public void PrintState()
        {
            Console.WriteLine($"QRCodeMatrix State (Size: {Size}, MaskPattern: {MaskPattern}):");

            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    Console.Write(_matrix[row, col] ? "X " : ". ");
                }
                Console.WriteLine();
            }
        }


        public void PrintStateComplicated()
        {

            for (int row = 0; row < Size; row++)
            {
                for (int col = 0; col < Size; col++)
                {
                    if (_reserved[row, col])
                        Console.Write("R ");      // Зарезервований модуль
                    else if (_matrix[row, col])
                        Console.Write("X ");      // Модуль встановлений (чорний)
                    else
                        Console.Write(". ");      // Модуль не встановлений (білий)
                }
                Console.WriteLine();
            }
        }

    }
}
