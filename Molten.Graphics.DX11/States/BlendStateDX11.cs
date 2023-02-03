using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;

namespace Molten.Graphics
{
    /// <summary>Stores a blend state for use with a <see cref="CommandQueueDX11"/>.</summary>
    public unsafe class BlendStateDX11 : GraphicsBlendState
    {
        public class SurfaceBlendDX11 : RenderSurfaceBlend
        {
            BlendStateDX11 _parent = null;
            int _index;

            internal SurfaceBlendDX11(BlendStateDX11 parent, int index) 
            {
                _parent = parent;
                _index = index;
            }

            public override bool BlendEnable
            {
                get => _parent._desc.RenderTarget[_index].BlendEnable == 1;
                set
                {
                    int val = value ? 1 : 0;
                    if(_parent._desc.RenderTarget[_index].BlendEnable != val)
                    {
                        _parent._desc.RenderTarget[_index].BlendEnable = val;
                        _parent._dirty = true;
                    }
                }
            }

            public override bool LogicOpEnable
            {
                get => _parent._desc.RenderTarget[_index].LogicOpEnable > 0;
                set
                {
                    int val = value ? 1 : 0;
                    if (_parent._desc.RenderTarget[_index].LogicOpEnable != val)
                    {
                        _parent._desc.RenderTarget[_index].LogicOpEnable = val;
                        _parent._dirty = true;
                    }
                }
            }

            public override BlendType SrcBlend
            {
                get => (BlendType)_parent._desc.RenderTarget[_index].SrcBlend;
                set
                {
                    if (_parent._desc.RenderTarget[_index].SrcBlend != (Blend)value)
                    {
                        _parent._desc.RenderTarget[_index].SrcBlend = (Blend)value;
                        _parent._dirty = true;
                    }
                }
            }

            public override BlendType DestBlend
            {
                get => (BlendType)_parent._desc.RenderTarget[_index].DestBlend;
                set
                {
                    if (_parent._desc.RenderTarget[_index].DestBlend != (Blend)value)
                    {
                        _parent._desc.RenderTarget[_index].DestBlend = (Blend)value;
                        _parent._dirty = true;
                    }
                }
            }

            public override BlendOperation BlendOp
            {
                get => (BlendOperation)_parent._desc.RenderTarget[_index].BlendOp;
                set
                {
                    if (_parent._desc.RenderTarget[_index].BlendOp != (BlendOp)value)
                    {
                        _parent._desc.RenderTarget[_index].BlendOp = (BlendOp)value;
                        _parent._dirty = true;
                    }
                }
            }

            public override BlendType SrcBlendAlpha
            {
                get => (BlendType)_parent._desc.RenderTarget[_index].SrcBlendAlpha;
                set
                {
                    if (_parent._desc.RenderTarget[_index].SrcBlendAlpha != (Blend)value)
                    {
                        _parent._desc.RenderTarget[_index].SrcBlendAlpha = (Blend)value;
                        _parent._dirty = true;
                    }
                }
            }

            public override BlendType DestBlendAlpha
            {
                get => (BlendType)_parent._desc.RenderTarget[_index].DestBlendAlpha;
                set
                {
                    if (_parent._desc.RenderTarget[_index].DestBlendAlpha != (Blend)value)
                    {
                        _parent._desc.RenderTarget[_index].DestBlendAlpha = (Blend)value;
                        _parent._dirty = true;
                    }
                }
            }

            public override BlendOperation BlendOpAlpha
            {
                get => (BlendOperation)_parent._desc.RenderTarget[_index].BlendOpAlpha;
                set
                {
                    if (_parent._desc.RenderTarget[_index].BlendOpAlpha != (BlendOp)value)
                    {
                        _parent._desc.RenderTarget[_index].BlendOpAlpha = (BlendOp)value;
                        _parent._dirty = true;
                    }
                }
            }

            public override LogicOperation LogicOp
            {
                get => (LogicOperation)_parent._desc.RenderTarget[_index].LogicOp;
                set
                {
                    if (_parent._desc.RenderTarget[_index].LogicOp != (LogicOp)value)
                    {
                        _parent._desc.RenderTarget[_index].LogicOp = (LogicOp)value;
                        _parent._dirty = true;
                    }
                }
            }

            public override ColorWriteFlags RenderTargetWriteMask
            {
                get => (ColorWriteFlags)_parent._desc.RenderTarget[_index].RenderTargetWriteMask;
                set
                {
                    if (_parent._desc.RenderTarget[_index].RenderTargetWriteMask != (byte)value)
                    {
                        _parent._desc.RenderTarget[_index].RenderTargetWriteMask = (byte)value;
                        _parent._dirty = true;
                    }
                }
            }
        }

        public unsafe ID3D11BlendState1* NativePtr => _native;

        ID3D11BlendState1* _native;
        BlendDesc1 _desc;

        bool _dirty;

        protected override RenderSurfaceBlend CreateSurfaceBlend(int index)
        {
            return new SurfaceBlendDX11(this, index);
        }

        public BlendStateDX11(DeviceDX11 device, BlendStateDX11 source = null) : base(device, source)
        {
            _dirty = true;
        }

        protected override void OnApply(GraphicsCommandQueue cmd)
        {
            if (_native == null || _dirty)
            {
                _dirty = false;
                GraphicsRelease();

                // Create new state
                (cmd as CommandQueueDX11).DXDevice.Ptr->CreateBlendState1(ref _desc, ref _native);
                Version++;
            }
        }

        protected override void OnLogState(Logger log)
        {
            LogDescription(log, Device, _desc);
        }

        internal static void LogDescription(Logger log, GraphicsDevice device, BlendDesc1 desc)
        {
            log.Debug($"   DX11 {desc.GetType().Name}:");
            log.Debug($"      Alpha to Coverage: {desc.AlphaToCoverageEnable}");
            log.Debug($"      Independent Blend: {desc.IndependentBlendEnable}");

            for (int i = 0; i < device.Adapter.Capabilities.PixelShader.MaxOutResources; i++)
            {
                ref RenderTargetBlendDesc1 b = ref desc.RenderTarget[i];
                log.Debug($"      RT {i} Blend Enabled: {b.BlendEnable}");
                if (b.BlendEnable == 1)
                {
                    log.Debug($"          Src Blend: {b.SrcBlend}");
                    log.Debug($"          Src Blend Alpha: {b.SrcBlendAlpha}");
                    log.Debug($"          Dest Blend: {b.DestBlend}");
                    log.Debug($"          Dest Blend Alpha: {b.DestBlendAlpha}");
                    log.Debug($"          Blend Op: {b.BlendOp}");
                    log.Debug($"          Blend Op Alpha: {b.BlendOpAlpha}");
                    log.Debug($"          Logic Op Enabled: {b.LogicOpEnable}");
                    log.Debug($"          Logic Op: {b.LogicOp}");
                    log.Debug($"          Write Mask: {b.RenderTargetWriteMask}");
                }
            }
        }

        public static implicit operator ID3D11BlendState1*(BlendStateDX11 state)
        {
            return state._native;
        }

        public static implicit operator ID3D11BlendState*(BlendStateDX11 state)
        {
            return (ID3D11BlendState*)state._native;
        }

        public override void GraphicsRelease()
        {
            SilkUtil.ReleasePtr(ref _native);
        }

        public override bool AlphaToCoverageEnable
        {
            get => _desc.AlphaToCoverageEnable > 0;
            set
            {
                _desc.AlphaToCoverageEnable = value ? 1 : 0;
                _dirty = true;
            }
        }

        public override bool IndependentBlendEnable
        {
            get => _desc.IndependentBlendEnable > 0;
            set
            {
                _desc.IndependentBlendEnable = value ? 1 : 0;
                _dirty = true;
            }
        }
    }
}
