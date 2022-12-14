using System.Collections;

namespace Bite
{
    public readonly struct BitView : IReadOnlyList<bool>
    {
        private readonly BitArray? _array;
        private readonly Memory<byte> _bytes;

        public bool this[int index]
        {
            get
            {
                if (_array is not null)
                {
                    return _array[index];
                }

                var mask = BitOrder.GetBitMask(index & 7);
                var @byte = _bytes.Span[index >> 3];
                return (@byte & mask) != 0;
            }
            set 
            {
                if (_array is not null)
                {
                    _array[index] = value;
                    return;
                }

                var mask = BitOrder.GetBitMask(index & 7);
                ref byte @byte = ref _bytes.Span[index >> 3];
                if (value)
                {
                    @byte |= mask;
                }
                else
                {
                    @byte &= (byte)~mask;
                }
            }
        }

        /// <summary>
        /// The number of bits in the view.
        /// </summary>
        public int Count => _array?.Count ?? 8 * _bytes.Length;

        /// <summary>
        /// The bit order.
        /// </summary>
        public BitOrder BitOrder { get; }

        /// <summary>
        /// Create a new <see cref="BitView"/> over a <see cref="BitArray"/>.
        /// </summary>
        /// <param name="array"></param>
        public BitView(BitArray array)
        {
            _array = array;
            _bytes = default;
            BitOrder = BitOrder.Lsb0;
        }

        /// <summary>
        /// Create a new <see cref="BitView"/> over a series of bytes
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="bitOrder"></param>
        public BitView(Memory<byte> bytes, BitOrder bitOrder = BitOrder.Lsb0)
        {
            _array = null;
            _bytes = bytes;
            BitOrder = bitOrder;
        }

        /// <summary>
        /// Let <see cref="BitArray"/> be implicitly assignable.
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
