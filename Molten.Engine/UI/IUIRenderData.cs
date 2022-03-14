using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.UI
{
    public interface IUIRenderData
    {
        void Render(SpriteBatcher sb, UIBaseData data);
    }
}
