using System.Collections;

namespace Bite
{
    public readonly struct BitView : IReadOnlyList<bool>
    {
        private readonly BitArray? _array;
        private readonly ReadOnlyMemory<byte> _bytes;

        public bool this[int index] => _array?[index] ?? BitOrder.GetBit(_bytes.Span[index >> 3], index & 7);

        /// <summary>
        /// The number of bits in the view.
        /// </summary>
        public int Count => _array?.Count ?? 8 * _bytes.Length;

        /// <summary>
        /// The bit order used by the BitView
        /// </summary>
        public BitOrder BitOrder { get; }

        /// <summary>
        /// Create a new BitView over a BitArray
        /// </summary>
        /// <param name="array"></param>
        public BitView(BitArray array)
        {
            _array = array;
            _bytes = default;
            BitOrder = BitOrder.Lsb0;
        }

        /// <summary>
        /// Create a new BitView over a series of bytes
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="bitOrder"></param>
        public BitView(ReadOnlyMemory<byte> bytes, BitOrder bitOrder = BitOrder.Lsb0)
        {
            _array = null;
            _bytes = bytes;
            BitOrder = bitOrder;
        }

        /// <summary>
        /// Let BitArray be implicitly assignable to BitView
        /// </summary>
        /// <param name="array"></param>
        public static implicit operator BitView(BitArray array) => new(array);

        /// <summary>
        /// Get the bit values as a boolean array.
        /// </summary>
        /// <returns></returns>
        public bool[] ToArray()
        {
            var array = new bool[Count];
            var index = 0;
            foreach (var bit in this)
            {
                array[index++] = bit;
            }
            return array;
        }

        public IEnumerator<bool> GetEnumerator()
        {
            if (_array is not null)
            {
                static IEnumerable<bool> GetBits(BitArray array)
                {
                    for (var i = 0; i < array.Count; i++)
                    {
                        yield return array[i];
                    }
                }

                return GetBits(_array).GetEnumerator();
            }
            else
            {
                return new MemoryEnumerator(_bytes, BitOrder);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private struct MemoryEnumerator : IEnumerator<bool>
        {
            private readonly ReadOnlyMemory<byte> _bytes;
            private readonly BitOrder _bitOrder;
            private byte _currentByte;
            private int _position;
            private byte _mask;

            public bool Current => (_currentByte & _mask) != 0;

            object IEnumerator.Current => Current;

            public MemoryEnumerator(ReadOnlyMemory<byte> bytes, BitOrder bitOrder)
            {
                _bytes = bytes;
                _bitOrder = bitOrder;
                _currentByte = 0;
                _position = 0;
                _mask = 0;
            }

            public bool MoveNext()
            {
                if (_bitOrder == BitOrder.Msb0)
                {
                    _mask >>= 1;
                }
                else
                {
                    _mask <<= 1;
                }

                if (_mask == 0x00)
                {
                    if (_position == _bytes.Length)
                    {
                        return false;
                    }

                    _currentByte = _bytes.Span[_position];
                    _position++;

                    if (_bitOrder == BitOrder.Msb0)
                    {
                        _mask = 0x80;
                    }
                    else
                    {
                        _mask = 0x01;
                    }
                }

                return true;
            }

            public void Reset()
            {
                _position = 0;
                _currentByte = 0;
                _mask = 0;
            }

            public void Dispose()
            {
            }
        }
    }
}
