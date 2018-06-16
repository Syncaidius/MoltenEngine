using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>Represents an implementation of an updatable scene object.</summary>
    public interface IUpdatable
    {
        /// <summary>
        /// Gets or sets the scene the <see cref="IUpdatable"/> is attached to. This is automatically set by the scene it is added to, so it is best to avoid setting this manually.
        /// </summary>
        Scene Scene { get; set; }

        /// <summary>Called when a the <see cref="IUpdatable"/> is enabled and it's parent scene is updated.</summary>
        /// <param name="time">A <see cref="Timing"/> instance.</param>
        void Update(Timing time);
    }
}
