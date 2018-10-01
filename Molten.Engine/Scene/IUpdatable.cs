using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    /// <summary>Represents an implementation of an updatable scene object. 
    /// <see cref="Update(Timing)"/> is automatically called when the implemented object is added to a <see cref="Molten.Scene"/>.</summary>
    public interface IUpdatable : ISceneObject
    {
        /// <summary>Called when a the <see cref="IUpdatable"/> is enabled and it's parent scene is updated.</summary>
        /// <param name="time">A <see cref="Timing"/> instance.</param>
        void Update(Timing time);

        /// <summary>
        /// Gets or sets whether the object is enabled. If false, the object will not be updated.
        /// </summary>
        bool IsEnabled { get; set; }
    }
}
