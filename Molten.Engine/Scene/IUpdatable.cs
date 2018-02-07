using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>Represents an implementation of an updatable <see cref="ISprite"/> object.</summary>
    public interface IUpdatable
    {
        /// <summary>Called when </summary>
        /// <param name="time"></param>
        void Update(Timing time);
    }
}
