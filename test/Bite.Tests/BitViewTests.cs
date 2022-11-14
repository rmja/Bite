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
            var input = new BitView(bytes, bitOrder);

            // When
            var indexed = Enumerable.Range(0, 16).Select(i => input[i]).ToArray();
            var iterated = input.ToArray();

            // Then
            var expected = new[]
            {
                1, 1, 0, 0, 0, 0, 0, 0,
                1, 0, 0, 0, 0, 0, 0, 1,
            }.Select(Convert.ToBoolean);
            Assert.Equal(expected, indexed);
            Assert.Equal(expected, iterated);
        }
    }
}
