using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Font;

namespace Molten.Graphics
{
    public class ComputePassDX11 : ComputePass
    {
        public ComputePassDX11(ComputeTask task, string name) : 
            base(task, name) { }

        protected override void OnInitializeState(ref ComputeStateParameters parameters)
        {
            
        }
    }
}
