using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    internal unsafe abstract class PipeStateStage<T, N> : PipeStage
        where N : unmanaged
        where T : PipeBindable<N>
    {
        internal PipeStateStage(DeviceContext pipe) : base(pipe)
        {
            State = DefineSlot<T>(0, PipeBindTypeFlags.Input, $"{typeof(N).Name}");
        }

        internal void Bind()
        {
            if (State.Bind())
                BindState(State.BoundValue);
        }

        protected abstract void BindState(T state);

        /// <summary>Gets the <see cref="GraphicsDepthState"/> bind slot.</summary>
        public PipeSlot<T> State { get; }
    }
}
