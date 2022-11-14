using System.Buffers;
using System.Runtime.CompilerServices;

namespace Bite
{
    public struct BitWriter
    {
        private static readonly uint[] _masks = BitUtils.CreateMaskTable();
        private readonly IBufferWriter<byte> _byteWriter;
        private ulong _buffer = 0;
        private int _bitsInBuffer = 0;
        private int _writtenBytes = 0;

        /// <summary>
        /// The bit order used by the BitWriter./// <summary>
        /// The bit order used by the BitWriter
        /// </summary>
        public BitOrder BitOrder { get; }

        /// <summary>
        /// The number of bits written to the writer.
        /// </summary>
        public int WrittenBitCount => 8 * _writtenBytes + _bitsInBuffer;

        /// <summary>
        /// Create a new BitWriter using the provided <paramref name="byteWriter"/> as underlying storage.
        /// The bits are written using <paramref name="bitOrder"/>.
        /// </summary>
        /// <param name="byteWriter"></param>
        /// <param name="bitOrder"></param>
        public BitWriter(IBufferWriter<byte> byteWriter, BitOrder bitOrder = BitOrder.Lsb0)
        {
            _byteWriter = byteWriter;
            BitOrder = bitOrder;
        }

        /// <summary>
        /// Write <paramref name="bitCount"/> bits with <paramref name="value"/>.
        /// </summary>
        /// <param name="bitCount"></param>
        /// <param name="value"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBits(int bitCount, uint value)
        {
            if (bitCount == 32 && _bitsInBuffer == 0)
            {
                Emit(value);
                return;
            }

            value &= _masks[bitCount];
            if (BitOrder == BitOrder.Msb0)
            {
                _buffer = (_buffer << bitCount) | value;
            }
            else
            {
                _buffer |= (ulong)value << _bitsInBuffer;
            }
            _bitsInBuffer += bitCount;
            if (_bitsInBuffer >= 32)
            {
                Emit((uint)_buffer);
                _buffer >>= 32;
                _bitsInBuffer -= 32;
            }
        }

        /// <summary>
        /// Pad zeros up to the next byte boundary.
        /// </summary>
        /// <returns>The number of padded zeros</returns>
        public int Pad()
        {
            var bitCount = 8 - (_bitsInBuffer % 8);
            WriteBits(bitCount, 0);
            return bitCount;
        }

        /// <summary>
        /// Flush the internal buffer to the underlying byte writer.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void Flush()
        {
            if (_bitsInBuffer % 8 != 0)
            {
                throw new InvalidOperationException();
            }

            if (_bitsInBuffer >= 32)
            {
                Emit((uint)_buffer);
                _buffer >>= 32;
                _bitsInBuffer -= 32;
            }

            Emit((uint)_buffer, _bitsInBuffer / 8);

            _buffer = 0;
            _bitsInBuffer = 0;
        }

        private void Emit(uint value, int byteCount = 4)
        {
            var span = _byteWriter.GetSpan(byteCount);
            BitOrder.Write(span, value, byteCount);
            _byteWriter.Advance(byteCount);
            _writtenBytes += byteCount;
        }
    }
}
