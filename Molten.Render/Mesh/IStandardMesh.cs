using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>A mesh which implements a standard set of properties and methods, making it viable to use in deferred rendering.</summary>
    public interface IStandardMesh : ICustomMesh<GBufferVertex>, IDisposable
    {

    }
}
