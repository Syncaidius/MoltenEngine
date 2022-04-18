using Android.OS;
using Android.Views;

namespace Molten.Graphics
{
    // TODO inherit from an OpenGL-based texture
    public class AndroidViewSurface : INativeSurface
    {
        public event WindowSurfaceHandler OnHandleChanged;

        public event WindowSurfaceHandler OnParentChanged;

        public event WindowSurfaceHandler OnClose;

        public event WindowSurfaceHandler OnMinimize;

        public event WindowSurfaceHandler OnRestore;

        public event WindowSurfaceHandler OnFocusGained;

        public event WindowSurfaceHandler OnFocusLost;

        public event TextureHandler OnPreResize;

        public event TextureHandler OnPostResize;


        public View TargetView { get; private set; }

        public IMoltenAndroidActivity TargetActivity { get; private set; }

        ViewportF _vp;

        public AndroidViewSurface(IMoltenAndroidActivity activity)
        {
            TargetView = activity.TargetView;
            TargetActivity = activity;

            CalculateViewport();
            activity.OnTargetViewChanged += Activity_OnTargetViewChanged;
        }

        private void CalculateViewport()
        {
            if(TargetView != null && TargetView.Width > 0 && TargetView.Height > 0)
            {
                // TODO correctly calculate this. The View may not be located at 0,0 within it's parent.
                _vp = new ViewportF()
                {
                    X = 0,
                    Y = 0,
                    Width = TargetView.Width,
                    Height = TargetView.Height
                };
            }
            else
            {
                _vp = new ViewportF();

                // GetRealSize() was defined in JellyBeanMr1 / API 17 / Android 4.2
                if (Build.VERSION.SdkInt < BuildVersionCodes.JellyBeanMr1)
                {
                    _vp.Width = TargetActivity.UnderlyingActivity.Resources.DisplayMetrics.WidthPixels;
                    _vp.Height = TargetActivity.UnderlyingActivity.Resources.DisplayMetrics.HeightPixels;
                }
                else
                {
                    Android.Graphics.Point p = new Android.Graphics.Point();
                    TargetActivity.UnderlyingActivity.WindowManager.DefaultDisplay.GetRealSize(p);
                    _vp.Width = p.X;
                    _vp.Height = p.Y;
                }
            }
        }

        private void Activity_OnTargetViewChanged(View o)
        {
            TargetView = o;
            CalculateViewport();
            OnHandleChanged?.Invoke(this);
        }

        public void Present()
        {
            throw new NotImplementedException();
        }

        public void Dispatch(Action callback)
        {
            throw new NotImplementedException();
        }

        public void Clear(Color color)
        {
            throw new NotImplementedException();
        }

        public void Resize(uint newWidth, uint newHeight, uint newMipMapCount)
        {
            throw new NotImplementedException();
        }

        public Texture2DProperties Get2DProperties()
        {
            throw new NotImplementedException();
        }

        public void Resize(uint newWidth, uint newHeight, uint newMipMapCount, uint newArraySize, GraphicsFormat newFormat)
        {
            throw new NotImplementedException();
        }

        public void Resize(uint newWidth, uint newHeight)
        {
            throw new NotImplementedException();
        }

        public Texture1DProperties Get1DProperties()
        {
            throw new NotImplementedException();
        }

        public void Resize(uint newWidth, uint newMipMapCount, GraphicsFormat format)
        {
            throw new NotImplementedException();
        }

        public void Resize(uint newWidth)
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

        public void CopyTo(uint sourceLevel, uint sourceSlice, ITexture destination, uint destLevel, uint destSlice)
        {
            throw new NotImplementedException();
        }

        public void SetData(TextureData data, uint srcMipIndex, uint srcArraySlice, uint mipCount, uint arrayCount, uint destMipIndex = 0, uint destArraySlice = 0)
        {
            throw new NotImplementedException();
        }

        public void SetData<T>(uint level, T[] data, uint startIndex, uint count, uint pitch, uint arraySlice = 0) where T : unmanaged
        {
            throw new NotImplementedException();
        }

        public void SetData(TextureSlice data, uint mipLevel, uint arraySlice)
        {
            throw new NotImplementedException();
        }

        public void SetData<T>(RectangleUI area, T[] data, uint bytesPerPixel, uint level, uint arrayIndex = 0) where T : unmanaged
        {
            throw new NotImplementedException();
        }

        public void GetData(ITexture stagingTexture, Action<TextureData> callback)
        {
            throw new NotImplementedException();
        }

        public void GetData(ITexture stagingTexture, uint level, uint arrayIndex, Action<TextureSlice> callback)
        {
            throw new NotImplementedException();
        }

        public bool HasFlags(TextureFlags flags)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public string Title
        {
            get => TargetActivity.UnderlyingActivity.Title;
            set
            {
                TargetActivity.UnderlyingActivity.Title = value;
                TargetActivity.UnderlyingActivity.Window?.SetTitle(value);
            }
        }

        public string Name
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public bool IsFocused
        {
            get => TargetActivity.UnderlyingActivity.HasWindowFocus;
        }

        public WindowMode Mode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IntPtr? ParentHandle { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IntPtr? WindowHandle => throw new NotImplementedException();

        public Rectangle Bounds => throw new NotImplementedException();

        public bool Visible
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public TextureFlags Flags => throw new NotImplementedException();

        public GraphicsFormat DataFormat => throw new NotImplementedException();

        public bool IsBlockCompressed => throw new NotImplementedException();

        public uint Width => (uint)_vp.Width;

        public uint Height => (uint)_vp.Height;

        public uint MipMapCount => throw new NotImplementedException();

        public uint ArraySize => throw new NotImplementedException();

        public AntiAliasLevel MultiSampleLevel => throw new NotImplementedException();

        public bool IsMultisampled => throw new NotImplementedException();

        public object Tag { get; set; }

        public RenderService Renderer => throw new NotImplementedException();

        public IntPtr Handle => throw new NotImplementedException();

        public ViewportF Viewport => _vp;
    }
}
