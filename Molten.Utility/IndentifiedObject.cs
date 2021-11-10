using System.Threading;

namespace Molten
{
    public abstract class IdentifiedObject : EngineObject
    {
        static long _nextID = long.MinValue;

        /// <summary>
        /// Creates a new instance of <see cref="IdentifiedObject"/>.
        /// </summary>
        public IdentifiedObject()
        {
            ID = Interlocked.Increment(ref _nextID);
        }

        /// <summary>
        /// Gets the ID assigned to the object. This ID is unique between all object types which inherit <see cref="IdentifiedObject"/>.
        /// </summary>
        public long ID { get; }
    }
}
