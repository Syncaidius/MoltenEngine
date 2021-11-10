namespace Molten.Utility.Collections
{
    public partial class ThreadedHashSet<T>
    {
        /// <summary>Used for set checking operations (using enumerables) that rely on counting</summary>
        internal struct ElementCount
        {
            internal int uniqueCount;
            internal int unfoundCount;
        }

        internal struct Slot
        {
            internal int hashCode;      // Lower 31 bits of hash code, -1 if unused
            internal T value;
            internal int next;          // Index of next entry, -1 if last
        }
    }
}
