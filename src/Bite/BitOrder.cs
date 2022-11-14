namespace Bite
{
    /// <summary>
    /// The bit ordering within a byte
    /// </summary>
    public enum BitOrder
    {
        /// <summary>
        /// Least-Significant-First
        /// This orders the bits in a byte with the least significant bit first and the most significant bit last.
        /// This matches the bit order order in other bcl types such as <see cref="System.Collections.BitArray"/>.
        /// </summary>
        Lsb0,

        /// <summary>
        /// Most-Significant-First
        /// This orders the bits in an element with the most significant bit first and the least significant bit last.
        /// This likely matches the ordering of bits you would expect to see in a debugger.
        /// </summary>
        Msb0,
    }
}
