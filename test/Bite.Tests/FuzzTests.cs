using Nerdbank.Streams;
using Xunit;

namespace Bite.Tests
{
    public class FuzzTests
    {
        [Theory]
        [InlineData(0x1337)]
        [InlineData(0xdead)]
        [InlineData(0xbeef)]
        public void CanWriteAndReadFuzz(int seed)
        {
            // Given
            const int trials = 10000;
            var random = new Random(seed);

            var pool = new FakePool(random.Next(20));
            var buffer = new Sequence<byte>(pool);
            var writer = new BitWriter(buffer);

            // When
            var written = new List<(int, uint)>(trials);
            for (var i = 0; i < trials; i++)
            {
                var bitCount = random.Next(32);
                var maxValue = (1 << bitCount) - 1;
                var value = (uint)random.Next(maxValue);

                writer.WriteBits(bitCount, value);
                written.Add((bitCount, value));
            }
            writer.Pad();
            writer.Flush();

            // Then
            var reader = new BitReader(buffer);
            for (var i = 0; i < trials; i++)
            {
                var (bitCount, expected) = written[i];

                var value = reader.ReadBits(bitCount);

                Assert.Equal(expected, value);
            }
        }
    }
}
