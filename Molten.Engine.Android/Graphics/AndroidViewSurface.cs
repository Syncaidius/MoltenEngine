using Android.OS;
using Android.Views;

namespace Molten.Graphics;

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

    public event TextureHandler OnResize;
    public event WindowSurfaceHandler OnMaximize;

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

    public void Dispatch(Action callback)
    {
        throw new NotImplementedException();
    }

    public void Resize(GraphicsPriority priority, uint newWidth, uint newHeight)
    {
        throw new NotImplementedException();
    }

    public void Resize(GraphicsPriority priority, uint newWidth, uint newHeight, uint newMipMapCount = 0, uint newArraySize = 0, GraphicsFormat newFormat = GraphicsFormat.Unknown)
    {
        throw new NotImplementedException();
    }

    public void Clear(GraphicsPriority priority, Color color)
    {
        throw new NotImplementedException();
    }

    public void GenerateMipMaps(GraphicsPriority priority, Action<GraphicsResource> completeCallback = null)
    {
        throw new NotImplementedException();
    }

    public void SetData(GraphicsPriority priority, TextureData data, uint srcMipIndex, uint srcArraySlice, uint mipCount, uint arrayCount, uint destMipIndex = 0, uint destArraySlice = 0, Action<GraphicsResource> completeCallback = null)
    {
        throw new NotImplementedException();
    }

    public void SetData<T>(GraphicsPriority priority, uint level, T[] data, uint startIndex, uint count, uint pitch, uint arraySlice = 0, Action<GraphicsResource> completeCallback = null) where T : unmanaged
    {
        throw new NotImplementedException();
    }

    public void SetData(GraphicsPriority priority, TextureSlice data, uint mipLevel, uint arraySlice, Action<GraphicsResource> completeCallback = null)
    {
        throw new NotImplementedException();
    }

    public void SetData<T>(GraphicsPriority priority, ResourceRegion area, T[] data, uint bytesPerPixel, uint level, uint arrayIndex = 0, Action<GraphicsResource> completeCallback = null) where T : unmanaged
    {
        throw new NotImplementedException();
    }

    public void GetData(GraphicsPriority priority, Action<TextureData> completeCallback = null)
    {
        throw new NotImplementedException();
    }

    public void GetData(GraphicsPriority priority, uint level, uint arrayIndex, Action<TextureSlice> completeCallback = null)
    {
        throw new NotImplementedException();
    }

    public void CopyTo(GraphicsPriority priority, GraphicsResource destination, Action<GraphicsResource> completeCallback = null)
    {
        throw new NotImplementedException();
    }

    public void CopyTo(GraphicsPriority priority, uint sourceLevel, uint sourceSlice, GraphicsResource destination, uint destLevel, uint destSlice, Action<GraphicsResource> completeCallback = null)
    {
        throw new NotImplementedException();
    }

    public void Apply(GraphicsQueue cmd)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public void Close()
    {
        throw new NotImplementedException();
    }

    public void GenerateMipMaps(GraphicsPriority priority, GraphicsTask.EventHandler completeCallback = null)
    {
        throw new NotImplementedException();
    }

    public void SetData(GraphicsPriority priority, TextureData data, uint srcMipIndex, uint srcArraySlice, uint mipCount, uint arrayCount, uint destMipIndex = 0, uint destArraySlice = 0, GraphicsTask.EventHandler completeCallback = null)
    {
        throw new NotImplementedException();
    }

    public void SetData<T>(GraphicsPriority priority, uint level, T[] data, uint startIndex, uint count, uint pitch, uint arraySlice = 0, GraphicsTask.EventHandler completeCallback = null) where T : unmanaged
    {
        throw new NotImplementedException();
    }

    public void SetData(GraphicsPriority priority, TextureSlice data, uint mipLevel, uint arraySlice, GraphicsTask.EventHandler completeCallback = null)
    {
        throw new NotImplementedException();
    }

    public void SetData<T>(GraphicsPriority priority, ResourceRegion area, T[] data, uint bytesPerPixel, uint level, uint arrayIndex = 0, GraphicsTask.EventHandler completeCallback = null) where T : unmanaged
    {
        throw new NotImplementedException();
    }

    public void CopyTo(GraphicsPriority priority, GraphicsResource destination, GraphicsTask.EventHandler completeCallback = null)
    {
        throw new NotImplementedException();
    }

    public void CopyTo(GraphicsPriority priority, uint sourceLevel, uint sourceSlice, GraphicsResource destination, uint destLevel, uint destSlice, GraphicsTask.EventHandler completeCallback = null)
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

    public bool IsFocused
    {
        get => TargetActivity.UnderlyingActivity.HasWindowFocus;
    }

    public WindowMode Mode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public IntPtr? ParentHandle { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public IntPtr? WindowHandle => throw new NotImplementedException();

    public Rectangle RenderBounds => throw new NotImplementedException();

    public bool IsVisible
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public GraphicsFormat DataFormat => throw new NotImplementedException();

    public bool IsBlockCompressed => throw new NotImplementedException();

    public uint Width => (uint)_vp.Width;

    public uint Height => (uint)_vp.Height;

    public uint Depth => 1;

    public uint MipMapCount => throw new NotImplementedException();

    public uint ArraySize => throw new NotImplementedException();

    public AntiAliasLevel MultiSampleLevel => throw new NotImplementedException();

    public bool IsMultisampled => throw new NotImplementedException();

    public object Tag { get; set; }

    public RenderService Renderer => throw new NotImplementedException();

    public ViewportF Viewport => _vp;

    public GraphicsDevice Device => throw new NotImplementedException();

    public uint Version { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public uint LastUsedFrameID => throw new NotImplementedException();

    public GraphicsResourceFlags Flags => throw new NotImplementedException();

    public GraphicsFormat ResourceFormat => throw new NotImplementedException();

    public MSAAQuality SampleQuality => throw new NotImplementedException();

    public TextureDimensions Dimensions => throw new NotImplementedException();

    public bool IsReleased => throw new NotImplementedException();

    public bool IsEnabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    GraphicsResourceHandle IGraphicsResource.Handle => throw new NotImplementedException();

    public ulong EOID => throw new NotImplementedException();

    public string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
}
