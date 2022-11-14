using System.Collections;

namespace Bite
{
    public static class BitUtils
    {
        internal static uint[] CreateMaskTable()
        {
            var masks = new uint[33];
            for (int i = 1; i < masks.Length - 1; i++)
            {
                uint num = (uint)((1 << i) - 1);
                masks[i] = num;
            }

            masks[32] = uint.MaxValue;
            return masks;
        }

        public static BitArray CreateBitArray(byte[] bytes, BitOrder bitOrder = BitOrder.Lsb0)
        {
            if (bitOrder == BitOrder.Msb0)
            {
                var reversed = new byte[bytes.Length];
                for (var i = 0; i < bytes.Length; i++)
                {
                    reversed[i] = ReverseBits(bytes[i]);
                }
                return new BitArray(reversed);
            }
            else
            {
                return new BitArray(bytes);
            }
        }

        public static byte ReverseBits(byte value)
        {
            // http://graphics.stanford.edu/~seander/bithacks.html#ReverseByteWith32Bits
            // https://stackoverflow.com/a/3590938/963753
            return (byte)(((value * 0x0802u & 0x22110u) | (value * 0x8020u & 0x88440u)) * 0x10101u >> 16);
        }
    }
}
