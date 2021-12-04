using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal abstract class PipeBindSlot : EngineObject
    {
        internal PipeBindSlot(PipeStage parent, uint slotID, PipeBindTypeFlags slotType)
        {
            Stage = parent;
            SlotID = slotID;
            SlotType = slotType;
        }

        internal abstract bool Bind();

        protected override void OnDispose()
        {
            // TODO Call PipeBindable.UnbindFrom(slot) o nbound object.
        }

        internal PipeStage Stage { get; }

        internal uint SlotID { get; }

        internal PipeBindTypeFlags SlotType { get; }
    }

    internal sealed class PipeBindSlot<T> : PipeBindSlot
        where T : PipeBindable
    {
        T _boundValue;
        uint _boundVersion;

        internal PipeBindSlot(PipeStage stage, uint slotID, PipeBindTypeFlags slotType) : 
            base(stage, slotID, slotType)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>True, if changes were detected.</returns>
        internal override bool Bind()
        {
            if (_boundValue != Value)
            {
                _boundValue?.UnbindFrom(this);
                _boundValue = Value;

                if (_boundValue != null)
                {
                    _boundVersion = _boundValue.Version;
                    _boundValue.BindTo(this);
                }

                Stage.Pipe.Profiler.Current.Bindings++;
                return true;
            }
            else if (Value != null)
            {
                if (_boundVersion != Value.Version)
                {
                    _boundVersion = Value.Version;
                    Stage.Pipe.Profiler.Current.Bindings++;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets or sets the value of the slot. This will be applied during the next pipe/context bind call.
        /// </summary>
        internal T Value { get; set; }
    }
}
