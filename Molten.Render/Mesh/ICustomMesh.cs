using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>An mesh containing un-indexed vertex data.</summary>
    /// <typeparam name="T">The type of vertex data that the mesh is to expect.</typeparam>
    public interface ICustomMesh<T> : IMesh where T : struct, IVertexType
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
