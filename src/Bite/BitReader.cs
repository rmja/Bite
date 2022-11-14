using System.Buffers;

namespace Bite
{
    public ref struct BitReader
    {
        private static readonly uint[] _masks = BitUtils.CreateMaskTable();
        private SequenceReader<byte> _sequence;
        private ulong _buffer = 0;
        private int _bitsInBuffer = 0;

        /// <summary>
        /// The bit order used by the BitReader.
        /// </summary>
        public BitOrder BitOrder { get; }

        /// <summary>
        /// The number of bits consumed by the reader.
        /// </summary>
        public int Position { get; private set; } = 0;
        
        /// <summary>
        /// The total number of bits available to the reader.
        /// </summary>
        public int BitCount => 8 * (int)_sequence.Length;

        public BitReader(ReadOnlyMemory<byte> bytes, BitOrder bitOrder = BitOrder.Lsb0)
            : this(new ReadOnlySequence<byte>(bytes), bitOrder)
        {
        }

        public BitReader(ReadOnlySequence<byte> sequence, BitOrder bitOrder = BitOrder.Lsb0)
        {
            _sequence = new SequenceReader<byte>(sequence);
            BitOrder = bitOrder;
        }

        /// <summary>
        /// Try and read <paramref name="bitCount"/> bits from the reader to <paramref name="value"/>.
        /// </summary>
        /// <param name="bitCount"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryReadBits(int bitCount, out uint value)
        {
            if (Position + bitCount > BitCount)
            {
                value = 0;
                return false;
            }

            value = ReadBits(bitCount);
            return true;
        }

        /// <summary>
        /// Read <paramref name="bitCount"/> bits from the reader.
        /// </summary>
        /// <param name="bitCount"></param>
        /// <returns></returns>
        public uint ReadBits(int bitCount)
        {
            Ensure(bitCount);

            uint value;
            var newBitsInBuffer = _bitsInBuffer - bitCount;
            if (BitOrder == BitOrder.Msb0)
            {
                value = (uint)(_buffer >> newBitsInBuffer);
                _buffer &= _masks[newBitsInBuffer];
            }
            else
            {
                value = (uint)_buffer & _masks[bitCount];
                _buffer >>= bitCount;
            }
            
            _bitsInBuffer = newBitsInBuffer;
            Position += bitCount;
            return value;
        }

        private void Ensure(int requiredBitCount)
        {
            if (requiredBitCount <= _bitsInBuffer)
            {
                return;
            }

            var value = BitOrder.Read(_sequence.UnreadSpan, out var consumed);
            _sequence.Advance(consumed);

            var bitCount = 8 * consumed;
            if (BitOrder == BitOrder.Msb0)
            {
                _buffer = (_buffer << bitCount) | value;
            }
            else
            {
                _buffer |= (ulong)value << _bitsInBuffer;
            }
            _bitsInBuffer += bitCount;

            if (requiredBitCount > _bitsInBuffer)
            {
                if (_sequence.End)
                {
                    throw new IndexOutOfRangeException();
                }

                // Continue with the next unread span
                Ensure(requiredBitCount);
            }
        }

        /// <summary>
        /// Get the bit values as a boolean array.
        /// </summary>
        /// <returns></returns>
        public bool[] ToArray()
        {
            var array = new bool[BitCount];
            var index = 0;
            foreach (var bit in this)
            {
                array[index++] = bit;
            }
            return array;
        }

        public void Reset()
        {
            _sequence = new SequenceReader<byte>(_sequence.Sequence);
            _buffer = 0;
            _bitsInBuffer = 0;
        }

        public Enumerator GetEnumerator() => new(this);

        public ref struct Enumerator
        {
            private BitReader _reader;

            public bool Current { get; private set; } = false;

            public Enumerator(BitReader reader)
            {
                _reader = reader;
            }

            public bool MoveNext()
            {
                var result = _reader.TryReadBits(1, out var value);
                Current = Convert.ToBoolean(value);
                return result;
            }

            public void Reset()
            {
                Current = false;
                _reader.Reset();
            }

            public void Dispose()
            {
            }
        }
    }
}
