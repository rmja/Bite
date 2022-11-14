using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Bite
{
    internal static class BitOrderExtensions
    {
        public static uint Read(this BitOrder bitOrder, ReadOnlySpan<byte> source, out int consumed)
        {
            uint value;
            consumed = Math.Min(source.Length, 4);
            if (consumed == 4)
            {
                value = BinaryPrimitives.ReadUInt32LittleEndian(source);
            }
            else
            {
                var byteRange = 0..consumed;
                if (bitOrder == BitOrder.Msb0)
                {
                    byteRange = (4 - consumed)..4;
                }

                Span<byte> padded = stackalloc byte[4];
                source.CopyTo(padded[byteRange]);
                value = BinaryPrimitives.ReadUInt32LittleEndian(padded);
            }

            if (bitOrder == BitOrder.Msb0)
            {
                value = BinaryPrimitives.ReverseEndianness(value);
            }

            return value;
        }

        public static void Write(this BitOrder bitOrder, Span<byte> buffer, uint value, int byteCount = 4)
        {
            var byteRange = 0..byteCount;
            if (bitOrder == BitOrder.Msb0)
            {
                value = BinaryPrimitives.ReverseEndianness(value);
                byteRange = (4 - byteCount)..4;
            }

            if (BitConverter.IsLittleEndian)
            {
                var bytes = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<uint, byte>(ref value), 4);
                bytes[byteRange].CopyTo(buffer);
            }
            else
            {
                Span<byte> bytes = stackalloc byte[4];
                BinaryPrimitives.WriteUInt32LittleEndian(bytes, value);
                bytes[byteRange].CopyTo(buffer);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool GetBit(this BitOrder bitOrder, byte value, int position) => bitOrder == BitOrder.Msb0
            ? (value & (0x80 >> position)) != 0
            : (value & (0x01 << position)) != 0;
    }
}
