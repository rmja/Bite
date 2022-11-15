using System.Buffers;
using System.Runtime.CompilerServices;

namespace Bite
{
    public ref struct BitReader
    {
        private static readonly uint[] _masks = BitUtils.CreateMaskTable();
        
        private readonly ReadOnlySpan<byte> _span;
        private int _spanPosition = 0;
        
        private readonly ReadOnlySequence<byte>? _sequence;
        private SequenceReader<byte> _sequenceReader;

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
        public int BitCount => 8 * ((int?)_sequence?.Length ?? _span.Length);

        public BitReader(byte[] bytes, BitOrder bitOrder = BitOrder.Lsb0)
            : this(bytes.AsSpan(), bitOrder)
        {
        }

        public BitReader(ReadOnlySpan<byte> bytes, BitOrder bitOrder = BitOrder.Lsb0)
        {
            _span = bytes;
            _sequence = null;
            _sequenceReader = default;
            BitOrder = bitOrder;
        }

        public BitReader(ReadOnlyMemory<byte> bytes, BitOrder bitOrder = BitOrder.Lsb0)
            : this(new ReadOnlySequence<byte>(bytes), bitOrder)
        {
        }

        public BitReader(ReadOnlySequence<byte> sequence, BitOrder bitOrder = BitOrder.Lsb0)
        {
            _span = default;
            _sequence = sequence;
            _sequenceReader = new SequenceReader<byte>(sequence);
            BitOrder = bitOrder;
        }

        /// <summary>
        /// Try and read a single bit from the reader to <paramref name="value"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryReadBit(out bool value)
        {
            if (!TryReadBits(1, out var bits))
            {
                value = default;
                return false;
            }

            value = bits != 0;
            return true;
        }

        /// <summary>
        /// Read a single bit from the reader.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ReadBit() => ReadBits(1) != 0;

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

            uint value;
            int consumed;
            if (_sequence.HasValue)
            {
                value = BitOrder.Read(_sequenceReader.UnreadSpan, out consumed);
                _sequenceReader.Advance(consumed);
            }
            else
            {
                value = BitOrder.Read(_span[_spanPosition..], out consumed);
                _spanPosition += consumed;
            }

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
                if (!_sequence.HasValue || _sequenceReader.End)
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
            if (_sequence.HasValue)
            {
                _sequenceReader = new SequenceReader<byte>(_sequence.Value);
            }
            else
            {
                _spanPosition = 0;
            }
            _buffer = 0;
            _bitsInBuffer = 0;
        }

        public Enumerator GetEnumerator() => new(this);

        public ref struct Enumerator
        {
            private BitReader _reader;
            private bool _current = false;

            public bool Current => _current;

            public Enumerator(BitReader reader)
            {
                _reader = reader;
            }

            public bool MoveNext() => _reader.TryReadBit(out _current);

            public void Reset()
            {
                _current = false;
                _reader.Reset();
            }

            public void Dispose()
            {
            }
        }
    }
}
