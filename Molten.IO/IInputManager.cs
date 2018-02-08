using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.IO
{
    public interface IInputManager
    {
        T GetHandler<T>(IWindowSurface surface) where T : InputHandlerBase, new();

        void SetActiveWindow(IWindowSurface surface);

        void Update(Timing time);
    }
}
