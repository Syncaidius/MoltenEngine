using System;

namespace Molten.Graphics
{
    [Flags]
    public enum ObjectUpdateFlags
    {
        /// <summary>The object will not update anything.</summary>
        None = 0,

        /// <summary>The object will update itself.</summary>
        Self = 1,

        /// <summary>The object will update it's children.</summary>
        Children = 2,

        /// <summary>All of the available flags combined.</summary>
        All = 3,
    }
}
