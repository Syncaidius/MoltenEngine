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
        /// <summary>Called when a the <see cref="IUpdatable"/> is enabled and it's parent scene is updated.</summary>
        /// <param name="time">A <see cref="Timing"/> instance.</param>
        void Update(Timing time);
    }
}
