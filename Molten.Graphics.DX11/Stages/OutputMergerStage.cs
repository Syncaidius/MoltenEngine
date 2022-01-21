using Silk.NET.Direct3D11;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal unsafe class OutputMergerStage : PipeStage   
    {
        PipeDX11 _pipe;

        GraphicsDepthWritePermission _boundDepthMode = GraphicsDepthWritePermission.Enabled;
        GraphicsDepthWritePermission _requestedDepthMode = GraphicsDepthWritePermission.Enabled;

        ID3D11RenderTargetView** _rtvs;
        uint _numRTVs;
        ID3D11DepthStencilView* _dsv;

        public OutputMergerStage(PipeDX11 pipe) : base(pipe.Device)
        {
            _pipe = pipe;

            uint maxRTs = Device.Features.SimultaneousRenderSurfaces;
            Surfaces = DefineSlotGroup<RenderSurface>(maxRTs, PipeBindTypeFlags.Output, "RT Output");
            DepthSurface = DefineSlot<DepthStencilSurface>(0, PipeBindTypeFlags.Output, "Depth-Stencil Output");

            _rtvs = EngineUtil.AllocPtrArray<ID3D11RenderTargetView>(maxRTs);
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            EngineUtil.FreePtrArray(ref _rtvs);
        }

        internal void Refresh()
        {
            bool rtChanged = Surfaces.BindAll();
            bool dsChanged = DepthSurface.Bind() || (_boundDepthMode != _requestedDepthMode);

            if(rtChanged || dsChanged)
            {
                if (rtChanged)
                {
                    _numRTVs = 0;

                    for (uint i = 0; i < Surfaces.SlotCount; i++)
                    {
                        if (Surfaces[i].BoundValue != null)
                        {
                            _numRTVs = i;
                            _rtvs[i] = Surfaces[i].BoundValue.RTV;
                        }
                        else
                        {
                            _rtvs[i] = null;
                        }
                    }
                }

                if (dsChanged)
                {
                    if (DepthSurface.BoundValue != null && _requestedDepthMode != GraphicsDepthWritePermission.Disabled) {
                        if (_requestedDepthMode == GraphicsDepthWritePermission.ReadOnly)
                            _dsv = DepthSurface.BoundValue.ReadOnlyView;
                        else
                            _dsv = DepthSurface.BoundValue.DepthView;
                    }
                    else
                    {
                        _dsv = null;
                    }

                    _boundDepthMode = _requestedDepthMode;
                }

                _pipe.Context->OMSetRenderTargets(_numRTVs, _rtvs, _dsv);
                Pipe.Profiler.Current.SurfaceBindings++;
            }
        }

        public GraphicsDepthWritePermission GetDepthMode()
        {
            return _requestedDepthMode;
        }

        /// <summary>Sets a list of render surfaces.</summary>
        /// <param name="surfaces">The surfaces.</param>
        /// <param name="count">The number of surfaces to set.</param>
        public void SetRenderSurfaces(RenderSurface[] surfaces, uint count)
        {
            if (surfaces != null)
            {
                for (uint i = 0; i < count; i++)
                    Surfaces[i].Value = surfaces[i];
            }
            else
            {
                count = 0;
            }

            // Set the remaining surfaces to null.
            for (uint i = count; i < Surfaces.SlotCount; i++)
                Surfaces[i].Value = null;
        }

        /// <summary>
        /// Sets the render surface.
        /// </summary>
        /// <param name="surface">The surface.</param>
        /// <param name="slot">The slot.</param>
        public void SetRenderSurface(RenderSurface surface, uint slot)
        {
            Surfaces[slot].Value = surface;
        }

        /// <summary>
        /// Fills the provided array with a list of applied render surfaces.
        /// </summary>
        /// <param name="destinationArray">The array to fill with applied render surfaces.</param>
        public void GetRenderSurfaces(RenderSurface[] destinationArray)
        {
            if (destinationArray.Length < Surfaces.SlotCount)
                throw new InvalidOperationException($"The destination array is too small ({destinationArray.Length}). A minimum size of {Surfaces.SlotCount} is needed.");
            for (uint i = 0; i < Surfaces.SlotCount; i++)
                destinationArray[i] = Surfaces[i].Value;
        }

        /// <summary>Gets the render surface located in the specified output slot.</summary>
        /// <param name="slot">The ID of the slot to retrieve from.</param>
        /// <returns></returns>
        public RenderSurface GetRenderSurface(uint slot)
        {
            return Surfaces[slot].Value;
        }

        /// <summary>
        /// Resets the render surfaces.
        /// </summary>
        /// <param name="resetMode">The reset mode.</param>
        /// <param name="outputOnFirst">If true and the reset mode is OutputSurface, it will only be applied to the first slot (0)..</param>
        public void ResetRenderSurfaces()
        {
            for (uint i = 0; i < Surfaces.SlotCount; i++)
                Surfaces[i].Value = null;
        }

        /// <summary>Clears a render target that is set on the device.</summary>
        /// <param name="color"></param>
        /// <param name="slot"></param>
        public void Clear(Color color, uint slot)
        {
            if (Surfaces[slot].Value != null)
                Surfaces[slot].Value.Clear(_pipe, color);
        }

        internal GraphicsValidationResult Validate()
        {
            // TODO if render target 0 is set, ensure a pixel shader is bound, otherwise flag as missing pixel shader.

            return GraphicsValidationResult.Successful;
        }

        public PipeSlotGroup<RenderSurface> Surfaces { get; }

        /// <summary>
        /// Gets or sets the output's depth mode.
        /// </summary>
        public GraphicsDepthWritePermission DepthWritePermission
        {
            get => _requestedDepthMode;
            set => _requestedDepthMode = value;
        }

        /// <summary>
        /// Gets or sets the output depth surface.
        /// </summary>
        public PipeSlot<DepthStencilSurface> DepthSurface { get; }
    }
}
