using System.Buffers;

namespace Bite.Tests
{
    internal class FakePool : MemoryPool<byte>
    {
        private readonly int _size;

        public override int MaxBufferSize => _size;

        public FakePool(int size)
        {
            _size = size;
        }

        public override IMemoryOwner<byte> Rent(int minBufferSize = -1)
        {
            if (minBufferSize == -1)
            {
                minBufferSize = _size;
            }
            return new Lease(new byte[minBufferSize]);
        }

        protected override void Dispose(bool disposing)
        {
        }

        class Lease : IMemoryOwner<byte>
        {
            public Memory<byte> Memory { get; }

            public Lease(Memory<byte> memory)
            {
                Memory = memory;
            }

            public void Dispose()
            {
            }
        }
    }
}
