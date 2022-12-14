using Xunit;

namespace Bite.Tests
{
    public class BitViewTests
    {
        [Theory]
        [InlineData(new byte[] { 0x03, 0x81 }, BitOrder.Lsb0)]
        [InlineData(new byte[] { 0xC0, 0x81 }, BitOrder.Msb0)]
        public void CanAccessAndIterate(byte[] bytes, BitOrder bitOrder)
        {
            // Given
            var view = new BitView(bytes, bitOrder);

            // When
            var indexed = Enumerable.Range(0, 16).Select(i => view[i]).ToArray();
            var iterated = view.ToArray();

            // Then
            var expected = new[]
            {
                1, 1, 0, 0, 0, 0, 0, 0,
                1, 0, 0, 0, 0, 0, 0, 1,
            }.Select(Convert.ToBoolean);
            Assert.Equal(expected, indexed);
            Assert.Equal(expected, iterated);
        }

        [Theory]
        [InlineData(new byte[] { 0x03, 0x81 }, BitOrder.Lsb0)]
        [InlineData(new byte[] { 0xC0, 0x81 }, BitOrder.Msb0)]
        public void CanAssign(byte[] expected, BitOrder bitOrder)
        {
            // Given
            var buffer = new byte[2];
            var input = new BitView(buffer, bitOrder);

            // When
            input[0] = true;
            input[1] = true;
            input[8] = true;
            input[15] = true;

            // Then
            Assert.Equal(expected, buffer);
        }
    }
}
