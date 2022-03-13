using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.UI.Components
{
    internal class UIPanel : UIComponent<UIPanelRenderer>
    {
        protected override void OnUpdate(Timing time)
        {
            throw new NotImplementedException();
        }
    }

    public class UIPanelRenderer : UIComponentRenderer
    {
        public override void Render()
        {
            throw new NotImplementedException();
        }
    }
}
