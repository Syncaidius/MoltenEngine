namespace Molten.Graphics;

/// <summary>A snapshot of an object's last rendered state.</summary>
public class ObjectRenderData
{
    /// <summary>The transform used during the last render frame.</summary>
    public Matrix4F LastTransform = Matrix4F.Identity;

    /// <summary>The transform we're supposed to be at.</summary>
    public Matrix4F TargetTransform = Matrix4F.Identity;

    /// <summary>The current transform after interpolation is applied. This is the one the object will be rendered with for the current frame.</summary>
    public Matrix4F RenderTransform = Matrix4F.Identity;

    /// <summary>The distance from the camera, calculated by the renderer using <see cref="RenderTransform"/>.</summary>
    public double DistanceFromCamera = 0;
}
