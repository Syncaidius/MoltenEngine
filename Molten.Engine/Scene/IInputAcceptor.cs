using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>Represents an implement of <see cref="ISprite"/> which also accepts input.</summary>
    public interface IInputAcceptor
    {
        /// <summary>Called when input needs to be processed on a sprite.</summary>
        /// <param name="time">The timing instance for the current thread.</param>
        void HandleInput(Engine engine, Timing time);
    }
}
