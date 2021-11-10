using Molten.Collections;
using Molten.Graphics.Textures;
using OpenGL;
using System;
using System.Threading;

namespace Molten.Graphics
{
    public delegate void TextureEvent(TextureBaseGL texture);

    public abstract class TextureBaseGL : PipelineObject<DeviceGL, DeviceGL>, ITexture
    {
        /// <summary>Triggered right before the internal texture resource is created.</summary>
        public event TextureEvent OnPreCreate;

        /// <summary>Triggered after the internal texture resource has been created.</summary>
        public event TextureEvent OnCreate;

        /// <summary>Triggered if the creation of the internal texture resource has failed (resulted in a null resource).</summary>
        public event TextureEvent OnCreateFailed;

        public event TextureHandler OnPreResize;

        public event TextureHandler OnPostResize;

        static int _nextSortKey = 0;

        ThreadedQueue<ITextureChange> _pendingChanges;
        uint _textureID;
        int _width;
        int _height;
        int _depth;
        int _mipLevels;
        int _arraySize;
        int _sampleCount;
        TextureTarget _target;
        GraphicsFormat _format;
        TextureFlags _flags;
        RendererGL _renderer;
        bool _isBlockCompressed;

        internal TextureBaseGL(RendererGL renderer,
            int width, int height, int depth, TextureTarget target,
            int mipLevels, int arraySize, int sampleCount,
            GraphicsFormat format, TextureFlags flags) :
            base(renderer.Device)
        {
            SortKey = Interlocked.Increment(ref _nextSortKey);
            _target = target;
            _width = width;
            _height = height;
            _depth = depth;
            _mipLevels = mipLevels;
            _arraySize = arraySize;
            _format = format;
            _flags = flags;
            _sampleCount = sampleCount;
            _renderer = renderer;
            _isBlockCompressed = BCHelper.GetBlockCompressed(_format);
            _pendingChanges = new ThreadedQueue<ITextureChange>();
        }

        protected void CreateTexture(bool resize)
        {
            OnPreCreate?.Invoke(this);
            OnDisposeForRecreation();
            CreateResource(ref _textureID, _target, resize);

            if (_textureID > 0)
            {
                //TrackAllocation();

                OnCreate?.Invoke(this);
            }
            else
            {
                OnCreateFailed?.Invoke(this);
            }
        }

        protected virtual void OnDisposeForRecreation()
        {
            OnDispose();
        }

        private protected override void OnPipelineDispose()
        {
            if (_textureID > 0)
            {
                Gl.BindTexture(_target, _textureID);
                Gl.DeleteTextures(_textureID);
                _textureID = 0;
            }
        }

        protected abstract void CreateResource(ref uint id, TextureTarget target, bool isResizing);

        public Texture1DProperties Get1DProperties()
        {
            return new Texture1DProperties()
            {
                ArraySize = _arraySize,
                Flags = _flags,
                Format = _format,
                MipMapLevels = _mipLevels,
                Width = _width,
            };
        }

        public void Resize(int newWidth, int newMipMapCount, GraphicsFormat format)
        {
            throw new NotImplementedException();
        }

        public void Resize(int newWidth)
        {
            throw new NotImplementedException();
        }

        public void GenerateMipMaps()
        {
            throw new NotImplementedException();
        }

        public void CopyTo(ITexture destination)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(int sourceLevel, int sourceSlice, ITexture destination, int destLevel, int destSlice)
        {
            throw new NotImplementedException();
        }

        public void SetData(TextureData data, int srcMipIndex, int srcArraySlice, int mipCount, int arrayCount, int destMipIndex = 0, int destArraySlice = 0)
        {
            throw new NotImplementedException();
        }

        public void SetData<T>(int level, T[] data, int startIndex, int count, int pitch, int arraySlice = 0) where T : struct
        {
            throw new NotImplementedException();
        }

        public void SetData(TextureData.Slice data, int mipLevel, int arraySlice)
        {
            throw new NotImplementedException();
        }

        public void SetData<T>(Rectangle area, T[] data, int bytesPerPixel, int level, int arrayIndex = 0) where T : struct
        {
            throw new NotImplementedException();
        }

        public void GetData(ITexture stagingTexture, Action<TextureData> callback)
        {
            throw new NotImplementedException();
        }

        public void GetData(ITexture stagingTexture, int level, int arrayIndex, Action<TextureData.Slice> callback)
        {
            throw new NotImplementedException();
        }

        public bool HasFlags(TextureFlags flags)
        {
            return (_flags & flags) == flags;
        }

        internal override void Refresh(DeviceGL device, PipelineBindSlot<DeviceGL, DeviceGL> slot)
        {
            Apply(device);
        }

        internal virtual void Apply(DeviceGL device)
        {
            if (IsDisposed)
                return;

            if (_textureID == 0)
                CreateTexture(false);

            // process all changes for the current pipe.
            while (_pendingChanges.Count > 0)
            {
                ITextureChange change = null;
                if (_pendingChanges.TryDequeue(out change))
                    change.Process(this);
            }
        }

        public MoltenRenderer Renderer => _renderer;

        public TextureFlags Flags => _flags;

        public GraphicsFormat Format => _format;

        public bool IsBlockCompressed => _isBlockCompressed;

        public int Width => _width;

        public int Height => _height;

        public int Depth => _depth;

        public int MipMapCount => _mipLevels;

        public int ArraySize => _arraySize;

        public int SampleCount => _sampleCount;

        public bool IsMultisampled => _sampleCount > 0;

        internal TextureTarget Target => _target;

        /// <summary>
        /// Gets the underlying OpenGL texture ID for the current texture instance.
        /// </summary>
        internal uint GLID { get; private protected set; }

        public int SortKey { get; }
    }
}
