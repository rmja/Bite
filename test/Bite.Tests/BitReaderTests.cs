using System.Buffers;
using System.Collections;
using Xunit;

namespace Bite.Tests
{
    public class BitReaderTests
    {
        [Theory]
        [InlineData(new byte[] { 0x1A }, BitOrder.Lsb0)]
        [InlineData(new byte[] { 0xB0 }, BitOrder.Msb0)]
        public void CanReadFewBits(byte[] bytes, BitOrder bitOrder)
        {
            // Given
            var reader = new BitReader(bytes, bitOrder);
            Assert.Equal(0, reader.Position);
            Assert.Equal(8, reader.BitCount);

            // When
            Assert.Equal(0b10u, reader.ReadBits(2));
            Assert.Equal(0b110u, reader.ReadBits(3));

            // Then
            Assert.Equal(5, reader.Position);
        }

        [Theory]
        [InlineData(new byte[] { 0xFA, 0xCB, 0xD1 }, BitOrder.Lsb0, false)]
        [InlineData(new byte[] { 0xFA, 0xCB, 0xD1 }, BitOrder.Lsb0, true)]
        [InlineData(new byte[] { 0xB6, 0x8E, 0x5F }, BitOrder.Msb0, false)]
        [InlineData(new byte[] { 0xB6, 0x8E, 0x5F }, BitOrder.Msb0, true)]
        public void CanReadBitsInMultipleBytes(byte[] bytes, BitOrder bitOrder, bool fragmentize)
        {
            // Given
            var sequence = fragmentize
                ? TestUtils.Fragmentize(bytes)
                : new ReadOnlySequence<byte>(bytes);
            var reader = new BitReader(sequence, bitOrder);
            Assert.Equal(0, reader.Position);
            Assert.Equal(24, reader.BitCount);

            // When
            Assert.Equal(0b10u, reader.ReadBits(2));
            Assert.Equal(0b110u, reader.ReadBits(3));
            Assert.Equal(0b1101000111001011111u, reader.ReadBits(19));

            // Then
            Assert.Equal(24, reader.Position);
        }

        [Theory]
        [InlineData(new byte[] { 0x5c, 0x06, 0x8d, 0xa5, 0x61, 0x83, 0xdb, 0x13 }, BitOrder.Lsb0)]
        [InlineData(new byte[] { 0x5c, 0x06, 0x8d, 0xa5, 0x61, 0x83, 0xdb, 0x13 }, BitOrder.Msb0)]
        public void CanEnumerateBooleans(byte[] input, BitOrder bitOrder)
        {
            // Given

            var reader = new BitReader(input, bitOrder);

            // When
            var bits = reader.ToArray();

            // Then
            if (bitOrder == BitOrder.Msb0)
            {
                var array = new BitArray(input.Select(BitUtils.ReverseBits).ToArray());
                var expected = array.Cast<bool>().ToArray();
                Assert.Equal(expected, bits);
            }
            else
            {
                var array = new BitArray(input);
                var expected = array.Cast<bool>().ToArray();
                Assert.Equal(expected, bits);
            }
        }
    }
}
