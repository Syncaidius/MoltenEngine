namespace Molten.Graphics;

public abstract partial class GraphicsDevice
{
    public abstract IRenderSurface2D CreateSurface(uint width, uint height, GraphicsFormat format = GraphicsFormat.R8G8B8A8_UNorm, 
        GraphicsResourceFlags flags = GraphicsResourceFlags.GpuWrite,
        uint mipCount = 1, uint arraySize = 1, AntiAliasLevel aaLevel = AntiAliasLevel.None, string name = null);

    public abstract IDepthStencilSurface CreateDepthSurface(uint width, uint height, DepthFormat format = DepthFormat.R24G8,
        GraphicsResourceFlags flags = GraphicsResourceFlags.GpuWrite,
        uint mipCount = 1, uint arraySize = 1, AntiAliasLevel aaLevel = AntiAliasLevel.None, string name = null);

    /// <summary>Creates a form with a surface which can be rendered on to.</summary>
    /// <param name="formTitle">The title of the form.</param>
    /// <param name="formName">The internal name of the form.</param>
    /// <param name="width">The width of the form.</param>
    /// <param name="height">The height of the form.</param>
    /// <param name="format">The format of the form surface.</param>
    /// <param name="mipCount">The number of mip map levels of the form surface.</param>
    /// <param name="enabled">Whether or not the form is enabled for presentation.</param>
    /// <returns></returns>
    public INativeSurface CreateFormSurface(string formTitle, string formName, uint width, uint height,
        GraphicsFormat format = GraphicsFormat.B8G8R8A8_UNorm,
        uint mipCount = 1, bool enabled = true)
    {
        INativeSurface surface = OnCreateFormSurface(formTitle, formName, width, height, format, mipCount);
        surface.IsEnabled = enabled;
        _outputSurfaces.Add(surface);
        return surface;
    }

    /// <summary>Creates a GUI control with a surface which can be rendered on to.</summary>
    /// <param name="controlTitle">The title of the form.</param>
    /// <param name="controlName">The internal name of the control.</param>
    /// <param name="mipCount">The number of mip map levels of the form surface.</param>
    /// <param name="enabled">Whether or not the control is enabled for presentation.</param>
    /// <returns></returns>
    public INativeSurface CreateControlSurface(string controlTitle, string controlName, uint mipCount = 1, bool enabled = true)
    {
        INativeSurface surface = OnCreateControlSurface(controlTitle, controlName, mipCount);
        surface.IsEnabled = enabled;
        _outputSurfaces.Add(surface);
        return surface;
    }

    /// <summary>Creates a form with a surface which can be rendered on to.</summary>
    /// <param name="formTitle">The title of the form.</param>
    /// <param name="formName">The internal name of the form.</param>
    /// <param name="width">The width of the form.</param>
    /// <param name="height">The height of the form.</param>
    /// <param name="mipCount">The number of mip map levels of the form surface.</param>
    /// <param name="format">The format of the form surface.</param>
    /// <returns></returns>
    protected abstract INativeSurface OnCreateFormSurface(string formTitle, string formName, uint width, uint height, 
        GraphicsFormat format = GraphicsFormat.B8G8R8A8_UNorm, 
        uint mipCount = 1);

    /// <summary>Creates a GUI control with a surface which can be rendered on to.</summary>
    /// <param name="controlTitle">The title of the form.</param>
    /// <param name="controlName">The internal name of the control.</param>
    /// <param name="mipCount">The number of mip map levels of the form surface.</param>
    /// <returns></returns>
    protected abstract INativeSurface OnCreateControlSurface(string controlTitle, string controlName, uint mipCount = 1);
}
