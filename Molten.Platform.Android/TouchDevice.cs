using Molten.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Platform.Android
{
    public class TouchDevice : IInputDevice
    {
        public bool IsConnected => throw new NotImplementedException();

        public string DeviceName => throw new NotImplementedException();

        public void ClearState()
        {
            throw new NotImplementedException();
        }

        public void OpenControlPanel()
        {
            throw new NotImplementedException();
        }
    }
}
