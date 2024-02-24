namespace Molten.Graphics;
public class ShaderBindManager
{
    ShaderBindPoint<ShaderResourceVariable>[][] _resources;
    ShaderBindPoint<ShaderSamplerVariable>[] _samplers;
    ShaderBindManager _parent;
    Shader _shader;

    internal ShaderBindManager(Shader shader, ShaderBindManager parent)
    {
        _shader = shader;
        _parent = parent;
        _resources = new ShaderBindPoint<ShaderResourceVariable>[Shader.BindTypes.Length][];
        _samplers = [];

        for (int i = 0; i < Shader.BindTypes.Length; i++)
            Resources[i] = [];
    }

    internal T Create<T>(string name, uint bindPoint, uint bindSpace, ShaderBindType bindType)
        where T: ShaderResourceVariable, new()
    {
        ShaderBindPoint bp = new(bindPoint, bindSpace);
        ref ShaderBindPoint<ShaderResourceVariable>[] points = ref Resources[(int)bindType];

        for (int i = 0; i < points.Length; i++)
        {
            if (points[i] == bp)
                return points[i].Object as T;
        }

        // First check if the parent has an existing bind-point for the same bind-slot and bind-space.
        T variable = null;
        if (_parent != null)
        {
            variable = _parent.Create<T>(name, bindPoint, bindSpace, bindType);
        }
        else // ...Otherwise create a new bind-point
        {
            variable = new T();
            variable.Name = name;
            variable.Parent = _shader;

            if (_shader.Variables.ContainsKey(name))
                _shader.Device.Log.Warning($"Duplicate cross-stage shader variable '{name}' detected -- Bind-point: {bindPoint} -- Bind-space: {bindSpace}");
            else
                _shader.Variables[name] = variable;
        }

        // Add new variable to bind points list.
        int index = points.Length;
        Array.Resize(ref points, points.Length + 1);
        points[index].Object = variable;

        return variable;
    }

    internal void Add(ShaderBindType type, ShaderResourceVariable variable, uint bindPoint, uint bindSpace = 0)
    {
        ShaderBindPoint<ShaderResourceVariable> result = default;
        Add(type, variable, bindPoint, bindSpace, ref result);
    }

    private void Add(
        ShaderBindType type,
        ShaderResourceVariable variable,
        uint bindPoint,
        uint bindSpace,
        ref ShaderBindPoint<ShaderResourceVariable> result)
    {
        ShaderBindPoint bp = new ShaderBindPoint(bindPoint, bindSpace);
        ref ShaderBindPoint<ShaderResourceVariable>[] points = ref _resources[(int)type];

        // Check if the current bind manager has a duplicate bind-point for the current bind type.
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i] == bp)
            {
                result = points[i];
                return;
            }
        }

        // If not, check the parent for a duplicate that we can reuse.
        int index = points.Length;
        EngineUtil.ArrayResize(ref points, index + 1);

        if (_parent != null)
            _parent.Add(type, variable, bindPoint, bindSpace, ref points[index]);
        else
            points[index] = new ShaderBindPoint<ShaderResourceVariable>(bindPoint, bindSpace, variable);
    }

    internal ShaderSamplerVariable Add(ShaderSampler sampler, uint bindPoint, uint bindSpace = 0)
    {
        ShaderSamplerVariable variable = new ShaderSamplerVariable();
        variable.Value = sampler;
        variable.IsImmutable = sampler.IsImmutable;

        int index = _samplers.Length;
        EngineUtil.ArrayResize(ref _samplers, index + 1);
        _samplers[index] = new ShaderBindPoint<ShaderSamplerVariable>(bindPoint, bindSpace, variable);

        return variable;
    }

    public ShaderBindPoint<ShaderResourceVariable>[][] Resources => _resources;

    public ShaderBindPoint<ShaderSamplerVariable>[] Samplers => _samplers;
}
