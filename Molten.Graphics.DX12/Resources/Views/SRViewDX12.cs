﻿using Silk.NET.Direct3D12;

namespace Molten.Graphics.DX12;
internal class SRViewDX12 : ViewDX12<ShaderResourceViewDesc>
{
    public SRViewDX12(ResourceHandleDX12 handle) : base(handle)
    { }

    protected unsafe override void OnCreate(ref ShaderResourceViewDesc desc)
    {
        Handle.Device.Ptr->CreateShaderResourceView(Handle.Ptr, desc, DescriptorHandle.CpuHandle);
    }

    private protected override void OnAllocateHandle(uint numDescriptors, out HeapHandleDX12 handle)
    {
        handle = Handle.Device.Heap.GetResourceHandle(numDescriptors);
    }
}