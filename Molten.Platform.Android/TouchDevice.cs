using Molten.Graphics;
using Molten.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Input
{
    public class TouchDevice : AndroidInputDeviceBase<TouchFinger>, ITouchDevice
    {
        public override bool IsConnected => throw new NotImplementedException();

        public override string DeviceName => throw new NotImplementedException();

        public FingerState GetFingerState(TouchFinger finger)
        {
            throw new NotImplementedException();
        }

        public int GetPressedFingerCount()
        {
            throw new NotImplementedException();
        }

        internal override void Bind(INativeSurface surface)
        {
            throw new NotImplementedException();
        }

        internal override void Unbind(INativeSurface surface)
        {
            throw new NotImplementedException();
        }

        internal override void Update(Timing time)
        {
            throw new NotImplementedException();
        }
    }
}
