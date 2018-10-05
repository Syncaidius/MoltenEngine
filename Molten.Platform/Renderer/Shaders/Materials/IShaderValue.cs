using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public interface IShaderValue
    {
        string Name { get; set; }

        object Value { get; set; }
    }
}
