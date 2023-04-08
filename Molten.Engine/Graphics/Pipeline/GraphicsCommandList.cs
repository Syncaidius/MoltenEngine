using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public abstract class GraphicsCommandList : EngineObject
    {
        /// <summary>
        /// A container for storing application data to share between completion callbacks of <see cref="HlslShader"/> passes.
        /// </summary>
        public class CustomDrawInfo
        {
            /// <summary>
            /// Custom compute dispatch group sizes. 
            /// <para>
            /// Any dimension that is 0 will default to the one provided by the shader's definition, if any.</para>
            /// </summary>
            public Vector3UI ComputeGroups;

            /// <summary>
            /// Gets a dictionary of custom values.
            /// </summary>
            public Dictionary<string, object> Values { get; } = new Dictionary<string, object>();

            public void Reset()
            {
                ComputeGroups = Vector3UI.Zero;
                Values.Clear();
            }
        }

        protected class BatchDrawInfo
        {
            public bool Began;

            public Vector3UI ComputeGroups;

            public CustomDrawInfo Custom { get; } = new CustomDrawInfo();

            public void Reset()
            {
                Began = false;
                Custom.Reset();
            }
        }

        protected GraphicsCommandList(GraphicsCommandQueue queue)
        {
            DrawInfo = new BatchDrawInfo();
            Queue = queue;
        }

        /// <summary>
        /// Starts recording commands in the current <see cref="GraphicsCommandList"/>.
        /// </summary>
        /// <param name="singleUse">If true, the command list can only be submitted once before being reset for reuse. 
        /// If false, the command list can be submitted more than once during the current frame. This is useful if you wish to reuse a set of recorded commands for multiple passes.</param>
        /// <exception cref="GraphicsCommandListException"></exception>
        public virtual void Begin(bool singleUse = true)
        {
#if DEBUG
            if (DrawInfo.Began)
                throw new GraphicsCommandListException(this, $"{nameof(GraphicsCommandList)}: EndDraw() must be called before the next BeginDraw() call.");
#endif

            DrawInfo.Began = true;
        }

        public virtual void End()
        {
#if DEBUG
            if (!DrawInfo.Began)
                throw new GraphicsCommandListException(this, $"{nameof(GraphicsCommandList)}: BeginDraw() must be called before EndDraw().");
#endif

            DrawInfo.Reset();
        }

        /// <summary>
        /// Adds a <see cref="GraphicsCommandList"/> to the current <see cref="GraphicsCommandList"/>.
        /// </summary>
        /// <param name="list">The command list to be added.</param>
        public abstract void Execute(params GraphicsCommandList[] list);

        /// <summary>
        /// Maps a resource to provide a <see cref="GraphicsStream"/> for reading or writing.
        /// </summary>
        /// <param name="resource">The resource to be mapped.</param>
        /// <param name="subresource">The sub-resource to be mapped. e.g. mip-map level or array slice.</param>
        /// <param name="offsetBytes">The number of bytes to offset the mapping. This sets the position of the returned <see cref="GraphicsStream"/>.</param>
        /// <param name="mapType">The type of mapping to perform.</param>
        /// <returns></returns>
        /// <exception cref="GraphicsResourceException"></exception>
        public unsafe GraphicsStream MapResource(GraphicsResource resource, uint subresource, uint offsetBytes, GraphicsMapType mapType)
        {
            if (resource.Stream != null)
                throw new GraphicsResourceException(resource, $"Cannot map a resource that is already mapped. Dispose of the provided {nameof(GraphicsStream)} first");

            ResourceMap map = GetResourcePtr(resource, subresource, mapType);
            resource.Stream = new GraphicsStream(this, resource, ref map);
            resource.Stream.Position = offsetBytes;
            return resource.Stream;
        }

        internal void UnmapResource(GraphicsResource resource)
        {
#if DEBUG
            if (resource.Stream == null)
                throw new GraphicsResourceException(resource, "$Cannot unmap a resource that has not been mapped yet. Call MapResource first");
#endif

            OnUnmapResource(resource, resource.Stream.SubResourceIndex);
            resource.Stream = null;
        }

        /// <summary>
        /// Invoked when a <see cref="GraphicsResource"/> is mapped for reading or writing by the CPU/system.
        /// </summary>
        /// <param name="resource">The <see cref="GraphicsResource"/></param>
        /// <param name="subresource">The sub-resource index. e.g. a texture mip-map level, or array slice.</param>
        /// <param name="mapType">The type of mapping to perform.</param>
        /// <returns></returns>
        protected abstract ResourceMap GetResourcePtr(GraphicsResource resource, uint subresource, GraphicsMapType mapType);

        protected abstract void OnUnmapResource(GraphicsResource resource, uint subresource);

        protected internal abstract unsafe void UpdateResource(GraphicsResource resource, uint subresource, ResourceRegion? region, void* ptrData, uint rowPitch, uint slicePitch);

        protected internal abstract void CopyResource(GraphicsResource src, GraphicsResource dest);

        public abstract unsafe void CopyResourceRegion(GraphicsResource source, uint srcSubresource, ResourceRegion* sourceRegion,
            GraphicsResource dest, uint destSubresource, Vector3UI destStart);

        /// <summary>Draw non-indexed, non-instanced primitives. 
        /// All queued compute shader dispatch requests are also processed</summary>
        /// <param name="shader">The <see cref="HlslShader"/> to apply when drawing.</param>
        /// <param name="vertexCount">The number of vertices to draw from the provided vertex buffer(s).</param>
        /// <param name="vertexStartIndex">The vertex to start drawing from.</param>
        public abstract GraphicsBindResult Draw(HlslShader shader, uint vertexCount, uint vertexStartIndex = 0);

        /// <summary>Draw instanced, unindexed primitives. </summary>
        /// <param name="shader">The <see cref="HlslShader"/> to apply when drawing.</param>
        /// <param name="vertexCountPerInstance">The expected number of vertices per instance.</param>
        /// <param name="instanceCount">The expected number of instances.</param>
        /// <param name="vertexStartIndex">The index of the first vertex.</param>
        /// <param name="instanceStartIndex">The index of the first instance element</param>
        public abstract GraphicsBindResult DrawInstanced(HlslShader shader,
            uint vertexCountPerInstance,
            uint instanceCount,
            uint vertexStartIndex = 0,
            uint instanceStartIndex = 0);

        /// <summary>Draw indexed, non-instanced primitives.</summary>
        /// <param name="shader">The <see cref="Shader"/> to apply when drawing.</param>
        /// <param name="vertexIndexOffset">A value added to each index before reading from the vertex buffer.</param>
        /// <param name="indexCount">The number of indices to be drawn.</param>
        /// <param name="startIndex">The index to start drawing from.</param>
        public abstract GraphicsBindResult DrawIndexed(HlslShader shader,
            uint indexCount,
            int vertexIndexOffset = 0,
            uint startIndex = 0);

        /// <summary>Draw indexed, instanced primitives.</summary>
        /// <param name="shader">The <see cref="Shader"/> to apply when drawing.</param>
        /// <param name="indexCountPerInstance">The expected number of indices per instance.</param>
        /// <param name="instanceCount">The expected number of instances.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="vertexIndexOffset">The index of the first vertex.</param>
        /// <param name="instanceStartIndex">The index of the first instance element</param>
        public abstract GraphicsBindResult DrawIndexedInstanced(HlslShader shader,
            uint indexCountPerInstance,
            uint instanceCount,
            uint startIndex = 0,
            int vertexIndexOffset = 0,
            uint instanceStartIndex = 0);

        /// <summary>
        /// Dispatches a <see cref="HlslShader"/> as a compute shader. Any non-compute passes will be skipped.
        /// </summary>
        /// <param name="shader">The shader to be dispatched.</param>
        /// <param name="groups">The number of thread groups.</param>
        /// <returns></returns>
        public abstract GraphicsBindResult Dispatch(HlslShader shader, Vector3UI groups);

        public GraphicsCommandQueue Queue { get; }

        protected BatchDrawInfo DrawInfo { get; }

        public GraphicsFence Fence { get; set; }
    }
}
