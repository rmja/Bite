# Bite

[![NuGet package](https://img.shields.io/nuget/v/Bite.svg)](https://nuget.org/packages/Bite)

Bite provides fast bit access with the types `BitReader`, `BitWriter`, and `BitView`.
It supports read and write of different memorepresentations in the form of `Lsb0` and `Msb0`.

## Examples
`BitReader` example:
```C#
var reader = new BitReader(new byte[] { 0x1A }, BitOrder.Lsb0);
// or var reader = new BitReader(new byte[] { 0xB0 }, BitOrder.Msb0);
Assert.Equal(0b10u, reader.ReadBits(2));
Assert.Equal(0b110u, reader.ReadBits(3));
```

`BitWriter` example:
```C#
var buffer = new ArrayBufferWriter<byte>();
var writer = new BitWriter(buffer, BitOrder.Lsb0);
writer.WriteBits(2, 0b10);
writer.WriteBits(3, 0b110);
writer.Pad(); // Write zeros up to the next byte boundary
writer.Flush();
Assert.Equal(new byte[] { 0x1A }, buffer.WrittenSpan.ToArray()); // Or 0xB0 for BitOrder.Msb0
```

`BitView` (or uquivalently `ReadOnlyBitView`) example:
```C#
var view = new BitView(new byte[] { 0x03, 0x81 }, BitOrder.Lsb0);
// or var view = new BitView(new byte[] { 0xC0, 0x81 }, BitOrder.Msb0);
Assert.True(view[0]);
Assert.True(view[1]);
Assert.False(view[2]);
// etc.
```
