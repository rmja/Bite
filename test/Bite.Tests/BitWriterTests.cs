using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Bite.Tests
{
    public class BitWriterTests
    {
        [Theory]
        [InlineData(0x1A, BitOrder.Lsb0)]
        [InlineData(0xB0, BitOrder.Msb0)]
        public void CanWriteFewBits(byte expected, BitOrder bitOrder)
        {
            // Given
            var buffer = new ArrayBufferWriter<byte>();
            var writer = new BitWriter(buffer, bitOrder);

            // When
            writer.WriteBits(2, 0b10);
            writer.WriteBits(3, 0b110);
            var filled = writer.Pad();
            writer.Flush();

            // Then
            Assert.Equal(3, filled);
            Assert.Equal(new byte[] { expected }, buffer.WrittenSpan.ToArray());
        }

        [Theory]
        [InlineData(new byte[] { 0xFA, 0xCB, 0xD1 }, BitOrder.Lsb0)]
        [InlineData(new byte[] { 0xB6, 0x8E, 0x5F }, BitOrder.Msb0)]
        public void CanWriteBitsInMultipleBytes(byte[] expected, BitOrder bitOrder)
        {
            // Given
            var buffer = new ArrayBufferWriter<byte>();
            var writer = new BitWriter(buffer, bitOrder);

            // When
            writer.WriteBits(2, 0b10);
            writer.WriteBits(3, 0b110);
            writer.WriteBits(19, 0b1101000111001011111);
            writer.Flush();

            // Then
            Assert.Equal(expected, buffer.WrittenSpan.ToArray());
        }
    }
}
