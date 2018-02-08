using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.UI
{
    /// <summary>A component that is simply used for containing other components. Does not render anything itself.</summary>
    public class UIContainer : UIComponent
    {
        public UIContainer(Engine engine) : base(engine)
        {

        }
    }
}
