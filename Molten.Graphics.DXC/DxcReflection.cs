using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    public abstract unsafe class DxcReflection : EngineObject
    {
        IDxcContainerReflection* _reflection;

        internal void SetDxcReflection(IDxcContainerReflection* reflection)
        {
            _reflection = reflection;
            LoadNativeReflection(_reflection);
        }

        /// <summary>
        /// Allows a native renderer (e.g. DX12 or vulkan) to implement it's own reflection retrieval on top of DXC container reflection.
        /// </summary>
        /// <param name="dxcReflection">The <see cref="IDxcContainerReflection"/> containing reflection data.</param>
        /// <remarks>See for example: https://github.com/microsoft/DirectXShaderCompiler/blob/cb5bb213d8780777fd2724f4dc875c9549e076df/tools/clang/unittests/HLSL/DxilContainerTest.cpp#L399</remarks>
        protected abstract void LoadNativeReflection(IDxcContainerReflection* dxcReflection);

        protected override void OnDispose()
        {
            SilkUtil.ReleasePtr(ref _reflection);
        }
    }
}
