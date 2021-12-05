using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public abstract class PipeBindSlot : EngineObject
    {
        internal PipeBindSlot(PipeStage parent, uint slotID, PipeBindTypeFlags slotType, string namePrefix)
        {
            Stage = parent;
            SlotID = slotID;
            SlotType = slotType;
            Name = $"{namePrefix} slot {SlotID}";
        }

        internal abstract bool Bind();

        internal PipeStage Stage { get; }

        internal uint SlotID { get; }

        internal PipeBindTypeFlags SlotType { get; }
    }

    internal sealed class PipeBindSlot<T> : PipeBindSlot
        where T : PipeBindable
    {
        uint _boundVersion;

        internal PipeBindSlot(PipeStage stage, uint slotID, PipeBindTypeFlags slotType, string namePrefix) : 
            base(stage, slotID, slotType, $"{namePrefix}_{typeof(T).Name}")
        {

        }

        protected override void OnDispose()
        {
            BoundValue.UnbindFrom(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>True, if changes were detected.</returns>
        internal override bool Bind()
        {
            if (BoundValue != Value)
            {
                BoundValue?.UnbindFrom(this);
                BoundValue = Value;

                if (BoundValue != null)
                {
                    _boundVersion = BoundValue.Version;
                    BoundValue.BindTo(this);
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

        internal T BoundValue { get; set; }
    }
}
