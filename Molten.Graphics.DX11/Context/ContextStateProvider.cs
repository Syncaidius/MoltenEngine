using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal abstract class ContextStateProvider : EngineObject
    {
        internal ContextStateProvider(DeviceContextState parent)
        {
            ParentState = parent;
        }

        /// <summary>
        /// Called when the current <see cref="ContextStateProvider"/> is to be bound to it's parent <see cref="DeviceContext"/>
        /// </summary>
        internal abstract void Bind();

        /// <summary>
        /// Gets the parent <see cref="DeviceContextState"/> that the current <see cref="ContextStateProvider"/> is bound to.
        /// </summary>
        internal DeviceContextState ParentState { get; }

        internal DeviceContext Context => ParentState.Context;
    }
}
