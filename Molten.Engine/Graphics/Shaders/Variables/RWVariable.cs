namespace Molten.Graphics
{
    public abstract class RWVariable : ShaderResourceVariable
    {
        protected override bool ValidateResource(GraphicsResource res)
        {
            if (res.IsUnorderedAccess && !(res is ISwapChainSurface))
                return true;
            else
                Parent.Device.Log.Warning($"Cannot use non-unordered-access or non-storage resource in ${nameof(RWVariable)}");

            return false;
        }
    }

    public class RWVariable<T> : RWVariable
    {
        protected override bool ValidateResource(GraphicsResource res)
        {
            return res is T && base.ValidateResource(res);
        }
    }
}
