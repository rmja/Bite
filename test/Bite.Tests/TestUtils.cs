using Nerdbank.Streams;
using System.Buffers;

namespace Bite.Tests
{
    internal class TestUtils
    {
        public static ReadOnlySequence<byte> Fragmentize(ReadOnlySpan<byte> bytes, int size = 1)
        {
            var pool = new FakePool(size);
            var writer = new Sequence<byte>(pool);

            for (var offset = 0; offset < bytes.Length; offset += size)
            {
                var remaining = bytes.Length - offset;
                var count = Math.Min(remaining, size);
                var span = writer.GetSpan(size);
                bytes.Slice(offset, count).CopyTo(span);
                writer.Advance(count);
            }

            return writer.AsReadOnlySequence;
        }
    }
}
