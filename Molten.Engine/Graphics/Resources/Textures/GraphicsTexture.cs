using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics.Textures;

namespace Molten.Graphics
{
    /// <summary>
    /// A delegate for texture event handlers.
    /// </summary>
    /// <param name="texture">The texture instance that triggered the event.</param>
    public delegate void TextureHandler(GraphicsTexture texture);

    public abstract class GraphicsTexture : GraphicsResource
    {            
        /// <summary>
        /// Invoked after resizing of the texture has completed.
        /// </summary>
        public event TextureHandler OnResize;

        protected GraphicsTexture(GraphicsDevice device, uint width, uint height, uint depth, uint mipCount,
                uint arraySize, AntiAliasLevel aaLevel, MSAAQuality sampleQuality, GraphicsFormat format, GraphicsResourceFlags flags, bool allowMipMapGen, string name) 
            : base(device, flags)
        {
            Name = string.IsNullOrWhiteSpace(name) ? $"{GetType().Name}_{width}x{height}" : name;
            IsMipMapGenAllowed = allowMipMapGen;
            ValidateFlagCombination();

            MSAASupport msaaSupport = MSAASupport.NotSupported; // TODO re-support. _renderer.Device.Features.GetMSAASupport(format, aaLevel);

            Width = width;
            Height = height;
            Depth = depth;
            MipMapCount = mipCount;
            ArraySize = arraySize;
            MultiSampleLevel = aaLevel > AntiAliasLevel.Invalid ? aaLevel : AntiAliasLevel.None;
            SampleQuality = msaaSupport != MSAASupport.NotSupported ? sampleQuality : MSAAQuality.Default;
            ResourceFormat = format;
            IsBlockCompressed = BCHelper.GetBlockCompressed(format);

            if (IsBlockCompressed)
                SizeInBytes = BCHelper.GetBCSize(format, width, height, mipCount) * arraySize;
            else
                SizeInBytes = (ResourceFormat.BytesPerPixel() * (width * height)) * arraySize;
        }

        protected void InvokeOnResize()
        {
            OnResize?.Invoke(this);
        }

        private void ValidateFlagCombination()
        {
            // Validate RT mip-maps
            if (IsMipMapGenAllowed)
            {
                if (Flags.Has(GraphicsResourceFlags.NoShaderAccess) || !(this is IRenderSurface2D))
                    throw new GraphicsResourceException(this, "Mip-map generation is only available on render-surface shader resources.");
            }

            // Only staging resources have CPU-write access.
            if (Flags.Has(GraphicsResourceFlags.CpuWrite))
            {
                if (!Flags.Has(GraphicsResourceFlags.NoShaderAccess))
                    throw new GraphicsResourceException(this, "Staging textures cannot allow shader access. Add GraphicsResourceFlags.NoShaderAccess flag.");

                // Staging buffers cannot have any other flags aside from 
                if (Flags != (GraphicsResourceFlags.CpuWrite | GraphicsResourceFlags.CpuRead | GraphicsResourceFlags.None | GraphicsResourceFlags.GpuWrite))
                    throw new GraphicsResourceException(this, "Staging textures must have all CPU/GPU read and write flags.");
            }
        }

        public unsafe void SetData<T>(GraphicsPriority priority, RectangleUI area, T* data, uint numElements, uint bytesPerPixel, uint level, uint arrayIndex = 0,
            Action<GraphicsResource> completeCallback = null)
            where T : unmanaged
        {
            uint texturePitch = area.Width * bytesPerPixel;
            uint pixels = area.Width * area.Height;
            uint expectedBytes = pixels * bytesPerPixel;
            uint dataBytes = (uint)(numElements * sizeof(T));

            if (pixels != numElements)
                throw new Exception($"The provided data does not match the provided area of {area.Width}x{area.Height}. Expected {expectedBytes} bytes. {dataBytes} bytes were provided.");

            // Do a bounds check
            RectangleUI texBounds = new RectangleUI(0, 0, Width, Height);
            if (!texBounds.Contains(area))
                throw new Exception("The provided area would go outside of the current texture's bounds.");

            QueueTask(priority, new TextureSetTask<T>(data, 0, numElements)
            {
                Pitch = texturePitch,
                StartIndex = 0,
                ArrayIndex = arrayIndex,
                MipLevel = level,
                Area = area,
                CompleteCallback = completeCallback,
            });
        }

        internal void OnSetSize(ref TextureResizeTask task)
        {
            // Avoid resizing/recreation if nothing has actually changed.
            if (Width == task.NewWidth &&
                Height == task.NewHeight &&
                Depth == task.NewDepth &&
                MipMapCount == task.NewMipMapCount &&
                ArraySize == task.NewArraySize &&
                ResourceFormat == task.NewFormat)
                return;

            Width = Math.Max(1, task.NewWidth);
            Height = Math.Max(1, task.NewHeight);
            Depth = Math.Max(1, task.NewDepth);
            MipMapCount = Math.Max(1, task.NewMipMapCount);
            ResourceFormat = task.NewFormat;

            OnSetSize();
            OnResize?.Invoke(this);
        }

        protected abstract void OnSetSize();

        public void CopyTo(GraphicsPriority priority, GraphicsTexture destination, Action<GraphicsResource> completeCallback = null)
        {
            if (ResourceFormat != destination.ResourceFormat)
                throw new ResourceCopyException(this, destination, "The source and destination texture formats do not match.");

            if (!destination.Flags.Has(GraphicsResourceFlags.GpuWrite))
                throw new ResourceCopyException(this, destination, "Cannoy copy to a buffer that does not have GPU-write permission.");

            // Validate dimensions.
            if (destination.Width != Width ||
                destination.Height != Height ||
                destination.Depth != Depth)
                throw new ResourceCopyException(this, destination, "The source and destination textures must have the same dimensions.");

            QueueTask(priority, new ResourceCopyTask()
            {
                Destination = destination,
                CompletionCallback = completeCallback,
            });
        }

        public void CopyTo(GraphicsPriority priority,
            uint sourceLevel, uint sourceSlice,
            GraphicsTexture destination, uint destLevel, uint destSlice,
            Action<GraphicsResource> completeCallback = null)
        {
            if (!destination.Flags.Has(GraphicsResourceFlags.GpuWrite))
                throw new ResourceCopyException(this, destination, "Cannoy copy to a buffer that does not have GPU-write permission.");

            if (ResourceFormat != destination.ResourceFormat)
                throw new ResourceCopyException(this, destination, "The source and destination texture formats do not match.");

            // Validate dimensions.
            // TODO this should only test the source and destination level dimensions, not the textures themselves.
            if (destination.Width != Width ||
                destination.Height != Height ||
                destination.Depth != Depth)
                throw new ResourceCopyException(this, destination, "The source and destination textures must have the same dimensions.");

            if (sourceLevel >= MipMapCount)
                throw new ResourceCopyException(this, destination, "The source mip-map level exceeds the total number of levels in the source texture.");

            if (sourceSlice >= ArraySize)
                throw new ResourceCopyException(this, destination, "The source array slice exceeds the total number of slices in the source texture.");

            if (destLevel >= destination.MipMapCount)
                throw new ResourceCopyException(this, destination, "The destination mip-map level exceeds the total number of levels in the destination texture.");

            if (destSlice >= destination.ArraySize)
                throw new ResourceCopyException(this, destination, "The destination array slice exceeds the total number of slices in the destination texture.");

            QueueTask(priority, new SubResourceCopyTask()
            {
                SrcRegion = null,
                SrcSubResource = (sourceSlice * MipMapCount) + sourceLevel,
                DestResource = destination,
                DestStart = Vector3UI.Zero,
                DestSubResource = (destSlice * destination.MipMapCount) + destLevel,
                CompletionCallback = completeCallback,
            });
        }

        /// <summary>Gets whether or not the texture is using a supported block-compressed format.</summary>
        public bool IsBlockCompressed { get; protected set; }

        /// <summary>Gets the width of the texture.</summary>
        public uint Width { get; protected set; }

        /// <summary>Gets the height of the texture.</summary>
        public uint Height { get; protected set; }

        /// <summary>Gets the depth of the texture. For a 3D texture this is the number of slices.</summary>
        public uint Depth { get; protected set; }

        /// <summary>Gets the number of mip map levels in the texture.</summary>
        public uint MipMapCount { get; protected set; }

        /// <summary>Gets the number of array slices in the texture. For a cube-map, this value will a multiple of 6. For example, a cube map with 2 array elements will have 12 array slices.</summary>
        public uint ArraySize { get; protected set; }

        public override uint SizeInBytes { get; }

        /// <summary>
        /// Gets the number of samples used when sampling the texture. Anything greater than 1 is considered as multi-sampled. 
        /// </summary>
        public AntiAliasLevel MultiSampleLevel { get; protected set; }

        /// <summary>
        /// Gets whether or not the texture is multisampled. This is true if <see cref="MultiSampleLevel"/> is at least <see cref="AntiAliasLevel.X2"/>.
        /// </summary>
        public bool IsMultisampled => MultiSampleLevel >= AntiAliasLevel.X2;

        public MSAAQuality SampleQuality { get; protected set; }

        public bool IsMipMapGenAllowed { get; }

        /// <inheritdoc/>
        public override GraphicsFormat ResourceFormat { get; protected set; }
    }
}
