using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>A base interface for mesh implementations.</summary>
    public interface IMesh : IRenderable, IDisposable
    {
        /// <summary>Gets whether or not the mesh was created as a dynamic mesh. 
        /// Dynamic meshes are preferable when the mesh's data will be changing at least once or more per frame.</summary>
        bool IsDynamic { get; }

        /// <summary>Gets the maximum number of vertices the mesh can contain.</summary>
        int MaxVertices { get; }

        /// <summary>Gets the number of vertices stored in the mesh.</summary>
        int VertexCount { get; }

        /// <summary>Gets the topology/structure of the mesh's data (e.g. line, triangles list/strip, etc).</summary>
        VertexTopology Topology { get; }

        /// <summary>Gets or sets the material applied to the current mesh.</summary>
        IMaterial Material { get; set; }
        
        /// <summary>Applies a shader resource to the mesh at the specified slot.</summary>
        /// <param name="res">The resource.</param>
        /// <param name="slot">The slot ID.</param>
        void SetResource(IShaderResource res, int slot);

        /// <summary>Gets the shader resource applied to the mesh at the specified slot.</summary>
        /// <param name="slot">The slot ID.</param>
        /// <returns>An <see cref="IShaderResource"/> that was applied at the specified slot.</returns>
        IShaderResource GetResource(int slot);
    }

    /// <summary>An mesh containing un-indexed vertex data.</summary>
    /// <typeparam name="T">The type of vertex data that the mesh is to expect.</typeparam>
    public interface IMesh<T> : IMesh where T : struct, IVertexType
    {
        /// <summary>Copies the provided vertex data to the current mesh.</summary>
        /// <typeparam name="I">The type of data to set.</typeparam>
        /// <param name="data">The data to be copied.</param>
        void SetVertices(T[] data);

        /// <summary>Copies the provided vertex data to the current mesh.</summary>
        /// <typeparam name="I">The type of data to set.</typeparam>
        /// <param name="count">The number of elements in the dat array to copy.</param>
        /// <param name="data">The data to be copied.</param>
        void SetVertices(T[] data, int count);

        /// <summary>Copies the provided vertex data to the current mesh.</summary>
        /// <typeparam name="I">The type of data to set.</typeparam>
        /// <param name="count">The number of elements in the dat array to copy.</param>
        /// <param name="data">The data to be copied.</param>
        /// <param name="startIndex">The element within the data array to start copying from.</param>
        void SetVertices(T[] data, int offset, int count);

    }
}
