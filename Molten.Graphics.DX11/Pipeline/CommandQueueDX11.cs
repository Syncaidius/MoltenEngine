using Molten.IO;
using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    internal delegate void ContextDrawCallback(MaterialPass pass);
    internal delegate void ContextDrawFailCallback(MaterialPass pass, uint iteration, uint passNumber, GraphicsBindResult result);

    /// <summary>Manages the pipeline of a either an immediate or deferred <see cref="CommandQueueDX11"/>.</summary>
    public unsafe partial class CommandQueueDX11 : GraphicsCommandQueue
    {
        ID3D11DeviceContext4* _context;
        ContextStateStack _stateStack;

        internal CommandQueueDX11(DeviceDX11 device, ID3D11DeviceContext4* context) :
            base(device)
        {
            _context = context;
            DXDevice = device;

            if (_context->GetType() == DeviceContextType.Immediate)
                Type = GraphicsContextType.Immediate;
            else
                Type = GraphicsContextType.Deferred;

            State = new DeviceContextState(this);

            _stateStack = new ContextStateStack(this);

            // Apply the surface of the graphics device's output initialally.
            State.SetRenderSurfaces(null);
        }

        /// <summary>
        /// Maps a resource on the current <see cref="CommandQueueDX11"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resource"></param>
        /// <param name="subresource"></param>
        /// <param name="mapType"></param>
        /// <param name="mapFlags"></param>
        /// <returns></returns>
        internal MappedSubresource MapResource<T>(T* resource, uint subresource, Map mapType, MapFlag mapFlags)
            where T : unmanaged
        {
            MappedSubresource mapping = new MappedSubresource();
            Native->Map((ID3D11Resource*)resource, subresource, mapType, (uint)mapFlags, ref mapping);

            return mapping;
        }

        /// <summary>
        /// Maps a resource on the current <see cref="CommandQueueDX11"/> and provides a <see cref="RawStream"/> to aid read-write operations.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resource"></param>
        /// <param name="subresource"></param>
        /// <param name="mapType"></param>
        /// <param name="mapFlags"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        internal MappedSubresource MapResource<T>(T* resource, uint subresource, Map mapType, MapFlag mapFlags, out RawStream stream)
            where T: unmanaged
        {
            MappedSubresource mapping = new MappedSubresource();
            Native->Map((ID3D11Resource*)resource, subresource, mapType, (uint)mapFlags, ref mapping);

            bool canWrite = mapType != Map.Read;
            bool canRead = mapType == Map.Read || mapType == Map.ReadWrite;
            stream = new RawStream(mapping.PData, uint.MaxValue, canRead, canWrite);

            return mapping;
        }

        internal void UnmapResource<T>(T* resource, uint subresource)
            where T : unmanaged
        {
            Native->Unmap((ID3D11Resource*)resource, subresource);
        }

        internal void CopyResourceRegion(
            ID3D11Resource* source, uint srcSubresource, ref Box sourceRegion, 
            ID3D11Resource* dest, uint destSubresource, Vector3UI destStart)
        {
            Native->CopySubresourceRegion(dest, destSubresource, destStart.X, destStart.Y, destStart.Z,
                source, srcSubresource, ref sourceRegion);

            Profiler.Current.CopySubresourceCount++;
        }

        internal void CopyResourceRegion(
            ID3D11Resource* source, uint srcSubresource, Box* sourceRegion,
            ID3D11Resource* dest, uint destSubresource, Vector3UI destStart)
        {
            Native->CopySubresourceRegion(dest, destSubresource, destStart.X, destStart.Y, destStart.Z,
                source, srcSubresource, sourceRegion);

            Profiler.Current.CopySubresourceCount++;
        }

        internal void UpdateResource(ID3D11Resource* resource, uint subresource, 
            Box* region, void* ptrData, uint rowPitch, uint slicePitch)
        {
            Native->UpdateSubresource(resource, subresource, region, ptrData, rowPitch, slicePitch);
            Profiler.Current.UpdateSubresourceCount++;
        }

        public int PushState()
        {
            return _stateStack.Push();
        }

        public void PopState()
        {
            _stateStack.Pop();
        }

        private GraphicsBindResult ApplyState(MaterialPass pass,
            GraphicsValidationMode mode,
            VertexTopology topology)
        {
            if (topology == VertexTopology.Undefined)
                return GraphicsBindResult.UndefinedTopology;

            State.Bind(pass, DrawInfo.Conditions, topology);

            // Validate all pipeline components.
            GraphicsBindResult result = State.Validate(mode);

            return result;
        }

        private GraphicsBindResult DrawCommon(Material mat, GraphicsValidationMode mode, VertexTopology topology, 
            ContextDrawCallback drawCallback, ContextDrawFailCallback failCallback)
        {
            GraphicsBindResult vResult = GraphicsBindResult.Successful;

            if (!DrawInfo.Began)
                throw new GraphicsCommandQueueException(this, $"{nameof(CommandQueueDX11)}: BeginDraw() must be called before calling {nameof(Draw)}()");

            State.Material.Value = mat;

            // Re-render the same material for mat.Iterations.
            for (uint i = 0; i < mat.Iterations; i++)
            {
                for (uint j = 0; j < mat.PassCount; j++)
                {
                    MaterialPass pass = mat.Passes[j];
                    vResult = ApplyState(pass, mode, topology);

                    if (vResult == GraphicsBindResult.Successful)
                    {
                        // Re-render the same pass for K iterations.
                        for (int k = 0; k < pass.Iterations; k++)
                        {
                            drawCallback(pass);
                            Profiler.Current.DrawCalls++;
                        }
                    }
                    else
                    {
                        failCallback(pass, i, j, vResult);
                        break;
                    }
                }
            }

            return vResult;
        }

        public override GraphicsBindResult Draw(IMaterial material, uint vertexCount, VertexTopology topology, uint vertexStartIndex = 0)
        {
            return DrawCommon(material as Material, GraphicsValidationMode.Unindexed, topology, (pass) =>
            {
                _context->Draw(vertexCount, vertexStartIndex);
            },
            (pass, iteration, passNumber, vResult) =>
            {
                DXDevice.Log.Warning($"Draw() call failed with result: {vResult} -- " + 
                    $"Iteration: M{iteration}/{material.Iterations}P{passNumber}/{material.PassCount} -- " +
                    $"Material: {material.Name} -- Topology: {topology} -- VertexCount: { vertexCount}");
            });
        }

        /// <inheritdoc/>
        public override GraphicsBindResult DrawInstanced(IMaterial material,
            uint vertexCountPerInstance,
            uint instanceCount,
            VertexTopology topology,
            uint vertexStartIndex = 0,
            uint instanceStartIndex = 0)
        {
            return DrawCommon(material as Material, GraphicsValidationMode.Instanced, topology, (pass) =>
            {
                _context->DrawInstanced(vertexCountPerInstance, instanceCount, vertexStartIndex, instanceStartIndex);
            },
            (pass, iteration, passNum, vResult) =>
            {
                DXDevice.Log.Warning($"DrawInstanced() call failed with result: {vResult} -- " + 
                        $"Iteration: M{iteration}/{material.Iterations}P{passNum}/{material.PassCount} -- Material: {material.Name} -- " +
                        $"Topology: {topology} -- VertexCount: { vertexCountPerInstance} -- Instances: {instanceCount}");
            });
        }

        /// <inheritdoc/>
        public override GraphicsBindResult DrawIndexed(IMaterial material,
            uint indexCount,
            VertexTopology topology,
            int vertexIndexOffset = 0,
            uint startIndex = 0)
        {
            return DrawCommon(material as Material, GraphicsValidationMode.Indexed, topology, (pass) =>
            {
                _context->DrawIndexed(indexCount, startIndex, vertexIndexOffset);
            },
            (pass, it, passNum, vResult) =>
            {
                DXDevice.Log.Warning($"DrawIndexed() call failed with result: {vResult} -- " +
                    $"Iteration: M{it}/{material.Iterations}P{passNum}/{material.PassCount}" +
                    $" -- Material: {material.Name} -- Topology: {topology} -- indexCount: { indexCount}");
            });
        }

        /// <inheritdoc/>
        public override GraphicsBindResult DrawIndexedInstanced(IMaterial material,
            uint indexCountPerInstance,
            uint instanceCount,
            VertexTopology topology,
            uint startIndex = 0,
            int vertexIndexOffset = 0,
            uint instanceStartIndex = 0)
        {
            return DrawCommon(material as Material, GraphicsValidationMode.InstancedIndexed, topology, (pass) =>
            {
                _context->DrawIndexedInstanced(indexCountPerInstance, instanceCount,
                    startIndex, vertexIndexOffset, instanceStartIndex);
            },
            (pass, it, passNum, vResult) =>
            {
                DXDevice.Log.Warning($"DrawIndexed() call failed with result: {vResult} -- " +
                    $"Iteration: M{it}/{material.Iterations}P{passNum}/{material.PassCount}" +
                    $" -- Material: {material.Name} -- Topology: {topology} -- Indices-per-instance: { indexCountPerInstance}");
            });
        }

        /// <inheritdoc/>
        public override void Dispatch(IComputeTask task, uint groupsX, uint groupsY, uint groupsZ)
        {
            bool csChanged = State.Bind(task as ComputeTask);

            if (State.CS.Shader.BoundValue == null)
            {
                return;
            }
            else
            {
                ComputeCapabilities comCap = Device.Adapter.Capabilities.Compute;

                if (groupsZ > comCap.MaxGroupCountZ)
                {
                    Device.Log.Error($"Unable to dispatch compute shader. Z dimension ({groupsZ}) is greater than supported ({comCap.MaxGroupCountZ}).");
                    return;
                }
                else if (groupsX > comCap.MaxGroupCountX)
                {
                    Device.Log.Error($"Unable to dispatch compute shader. X dimension ({groupsX}) is greater than supported ({comCap.MaxGroupCountX}).");
                    return;
                }
                else if (groupsY > comCap.MaxGroupCountY)
                {
                    Device.Log.Error($"Unable to dispatch compute shader. Y dimension ({groupsY}) is greater than supported ({comCap.MaxGroupCountY}).");
                    return;
                }

                // TODO have this processed during the presentation call of each graphics context.
                // 
                Native->Dispatch(groupsX, groupsY, groupsZ);
            }
        }

        /// <summary>Dispoes of the current <see cref="Graphics.CommandQueueDX11"/> instance.</summary>
        protected override void OnDispose()
        {
            State.Dispose();

            // Dispose context.
            if (Type != GraphicsContextType.Immediate)
            {
                SilkUtil.ReleasePtr(ref _context);
                DXDevice.RemoveDeferredContext(this);
            }
        }

        /// <summary>Gets the current <see cref="CommandQueueDX11"/> type. This value will not change during the context's life.</summary>
        public GraphicsContextType Type { get; private set; }

        internal DeviceDX11 DXDevice { get; private set; }

        internal ID3D11DeviceContext4* Native => _context;

        /// <summary>
        /// Gets the state of the current <see cref="CommandQueueDX11"/>.
        /// </summary>
        internal DeviceContextState State { get; private set; }
    }
}
