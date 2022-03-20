namespace Molten.Net
{
    public enum DeliveryMethod : byte
    {
        Unknown = 0,

        /// No guarantees, except for preventing duplicates.
        Unreliable = 1,

        /// Late messages will be dropped if newer ones were already received.
        UnreliableSequenced = 2,

        /// All packages will arrive, but not necessarily in the same order.
        ReliableUnordered = 3,

        /// All packages will arrive, but late ones will be dropped.
        ReliableSequenced = 4,

        /// This means that we will always receive the latest message eventually, but may miss older ones.
        /// Unlike all the other methods, here the library will hold back messages until all previous ones are received, before handing them to us.
        /// All packages will arrive, and they will do so in the same order.
        ReliableOrdered = 5,
    }
}
