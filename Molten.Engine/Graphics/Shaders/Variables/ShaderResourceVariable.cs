namespace Molten.Graphics
{
    public abstract class ShaderResourceVariable : ShaderVariable
    {
        GraphicsResource _resource;

        protected abstract bool ValidateResource(GraphicsResource value);

        /// <summary>Gets the resource bound to the variable.</summary>
        public GraphicsResource Resource => _resource;

        /// <summary>
        /// Gets or sets the value of the resource variable.
        /// </summary>
        public override object Value
        {
            get => _resource;
            set
            {
                if (value != _resource)
                {
                    if (value != null)
                    {
                        if (value is GraphicsResource res && ValidateResource(res))
                            _resource = res;
                        else
                            Parent.Device.Log.Warning($"Cannot set '{value.GetType().Name}' object on resource variable '{Name}'");
                    }
                    else
                    {
                        _resource = null;
                    }
                }
            }
        }
    }

    public class ShaderResourceVariable<T> : ShaderResourceVariable
    {
        protected override bool ValidateResource(GraphicsResource value)
        {
            return value is T;
        }
    }
}
