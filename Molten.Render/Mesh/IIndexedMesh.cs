using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public interface IIndexedMesh<T> : IMesh<T> where T : struct, IVertexType
    {
        void SetIndices<I>(I[] data) where I : struct;

        void SetIndices<I>(I[] data, int count) where I : struct;

        void SetIndices<I>(I[] data, int offsetIndex, int count) where I : struct;

        /// <summary>Gets the maximum number of indices the mesh can contain.</summary>
        int MaxIndices { get; }
    }
}
