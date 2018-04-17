using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public interface IMaterialPass
    {
        string Name { get; }

        int Iterations { get; set; }

        IMaterial Material { get; }
    }
}
