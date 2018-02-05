using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public interface IMesh : IRenderable, IDisposable
    {
        /// <summary>Gets the maximum number of vertices the mesh can contain.</summary>
        int MaxVertices { get; }

        /// <summary>Gets the topology/structure of the mesh's data (e.g. line, triangles list/strip, etc).</summary>
        VertexTopology Topology { get; }

        /// <summary>Gets or sets the material applied to the current mesh.</summary>
        IMaterial Material { get; set; }
        
        void SetResource(IShaderResource res, int slot);

        IShaderResource GetResource(int slot);
    }

    public interface IMesh<T> : IMesh where T : struct, IVertexType
    {
        void SetVertices(T[] data);

        void SetVertices(T[] data, int count);

        void SetVertices(T[] data, int offset, int count);
    }
}
