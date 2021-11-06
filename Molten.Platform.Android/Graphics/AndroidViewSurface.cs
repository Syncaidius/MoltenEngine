using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Molten.Graphics
{
    // TODO inherit from an OpenGL-based surface
    public class AndroidViewSurface : INativeSurface
    {
        View _view;
        Activity _activity;

        public AndroidViewSurface(View view, Activity activity)
        {
            _view = view;
            _activity = activity;
        }

        public string Title
        {
            get => _activity.Title;
            set
            {
                _activity.Title = value;
                _activity.Window?.SetTitle(value);
            }
        }

        public string Name
        {
            get => throw new NotImplementedException(); 
            set => throw new NotImplementedException();
        }

        public bool IsFocused
        {
            get => _activity.HasWindowFocus;
        }

        public WindowMode Mode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IntPtr? ParentHandle { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IntPtr? WindowHandle => throw new NotImplementedException();

        public Rectangle Bounds => throw new NotImplementedException();

        public bool Visible { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Viewport Viewport => throw new NotImplementedException();

        public int Height => throw new NotImplementedException();

        public TextureFlags Flags => throw new NotImplementedException();

        public GraphicsFormat Format => throw new NotImplementedException();

        public bool IsBlockCompressed => throw new NotImplementedException();

        public int Width => _view.Width;

        public int MipMapCount => throw new NotImplementedException();

        public int ArraySize => throw new NotImplementedException();

        public int SampleCount => throw new NotImplementedException();

        public bool IsMultisampled => throw new NotImplementedException();

        public object Tag { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public int SortKey => throw new NotImplementedException();

        public MoltenRenderer Renderer => throw new NotImplementedException();

        public event WindowSurfaceHandler OnHandleChanged;
        public event WindowSurfaceHandler OnParentChanged;
        public event WindowSurfaceHandler OnClose;
        public event WindowSurfaceHandler OnMinimize;
        public event WindowSurfaceHandler OnRestore;
        public event WindowSurfaceHandler OnFocusGained;
        public event WindowSurfaceHandler OnFocusLost;
        public event TextureHandler OnPreResize;
        public event TextureHandler OnPostResize;

        public void Clear(Color color)
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

        public void Dispatch(Action callback)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void GenerateMipMaps()
        {
            throw new NotImplementedException();
        }

        public Texture1DProperties Get1DProperties()
        {
            throw new NotImplementedException();
        }

        public Texture2DProperties Get2DProperties()
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
            throw new NotImplementedException();
        }

        public void Present()
        {
            throw new NotImplementedException();
        }

        public void Resize(int newWidth, int newHeight, int newMipMapCount)
        {
            throw new NotImplementedException();
        }

        public void Resize(int newWidth, int newHeight, int newMipMapCount, int newArraySize, GraphicsFormat newFormat)
        {
            throw new NotImplementedException();
        }

        public void Resize(int newWidth, int newHeight)
        {
            throw new NotImplementedException();
        }

        public void Resize(int newWidth, int newMipMapCount, GraphicsFormat format)
        {
            throw new NotImplementedException();
        }

        public void Resize(int newWidth)
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
    }
}