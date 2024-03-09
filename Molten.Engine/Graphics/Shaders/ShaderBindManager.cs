using Molten.Graphics.Textures;
using Silk.NET.Core.Native;

namespace Molten.Graphics;
public class ShaderBindManager
{
    public delegate void OnFindVariableCallback(ref ShaderBind<ShaderResourceVariable> bindPoint);

    ShaderBind<ShaderResourceVariable>[][] _resources;
    ShaderBind<ShaderSamplerVariable>[] _samplers;
    ShaderBindManager _parent;
    Shader _shader;

    internal ShaderBindManager(Shader shader, ShaderBindManager parent)
    {
        _shader = shader;
        _parent = parent;
        _resources = new ShaderBind<ShaderResourceVariable>[Shader.BindTypes.Length][];
        _samplers = [];

        for (int i = 0; i < Shader.BindTypes.Length; i++)
            Resources[i] = [];
    }

    internal T Create<T>(string name, uint bindPoint, uint bindSpace, ShaderBindType bindType)
        where T: ShaderResourceVariable, new()
    {
        ShaderBindInfo bp = new(bindPoint, bindSpace);
        ref ShaderBind<ShaderResourceVariable>[] points = ref Resources[(int)bindType];

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
        points[index] = new ShaderBind<ShaderResourceVariable>(bindPoint, bindSpace, variable);

        return variable;
    }

    internal void Add(ShaderBindType type, ShaderResourceVariable variable, uint bindPoint, uint bindSpace = 0)
    {
        ShaderBind<ShaderResourceVariable> result = default;
        Add(type, variable, bindPoint, bindSpace, ref result);
    }

    private void Add(
        ShaderBindType type,
        ShaderResourceVariable variable,
        uint bindPoint,
        uint bindSpace,
        ref ShaderBind<ShaderResourceVariable> result)
    {
        ShaderBindInfo bp = new ShaderBindInfo(bindPoint, bindSpace);
        ref ShaderBind<ShaderResourceVariable>[] points = ref _resources[(int)type];

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
        Array.Resize(ref points, index + 1);

        if (_parent != null)
            _parent.Add(type, variable, bindPoint, bindSpace, ref points[index]);
        else
            points[index] = new ShaderBind<ShaderResourceVariable>(bindPoint, bindSpace, variable);

        result = points[index];
    }

    internal void Add(ShaderSampler sampler, uint bindPoint, uint bindSpace)
    {
        ShaderBind<ShaderSamplerVariable> result = default;
        Add(sampler, bindPoint, bindSpace, ref result);
    }

    internal void Add(ShaderSampler sampler, uint bindPoint, uint bindSpace, ref ShaderBind<ShaderSamplerVariable> result)
    {
        ShaderBindInfo bp = new ShaderBindInfo(bindPoint, bindSpace);

        // Check if the current bind manager has a duplicate bind-point for the current bind type.
        for (int i = 0; i < _samplers.Length; i++)
        {
            if (_samplers[i] == bp)
            {
                result = _samplers[i];
                return;
            }
        }

        // If not, check the parent for a duplicate that we can reuse.
        int index = _samplers.Length;
        Array.Resize(ref _samplers, index + 1);

        if (_parent != null)
        {
            _parent.Add(sampler, bindPoint, bindSpace, ref _samplers[index]);
        }
        else
        {
            ShaderSamplerVariable variable = new ShaderSamplerVariable();
            variable.Value = sampler;
            variable.IsImmutable = sampler.IsImmutable;
            _samplers[index] = new ShaderBind<ShaderSamplerVariable>(bindPoint, bindSpace, variable);
        }

        result = _samplers[index];
    }

    /// <summary>
    /// Searches for the provided variable and 
    /// </summary>
    /// <param name="variableToFind"></param>
    /// <param name="onFound"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public void OnFind(ShaderResourceVariable variableToFind, OnFindVariableCallback onFound)
    {
        if (variableToFind == null)
            throw new ArgumentNullException(nameof(variableToFind));

        if (onFound == null)
            throw new ArgumentNullException(nameof(onFound));

        for (int i = 0; i < Resources.Length; i++)
        {
            ShaderBind<ShaderResourceVariable>[] list = Resources[i];
            for (int j = 0; j < list.Length; j++)
            {
                if (list[i].Object == variableToFind)
                    onFound(ref list[i]);
            }
        }
    }

    public ShaderBind<ShaderResourceVariable>[][] Resources => _resources;

    public ShaderBind<ShaderSamplerVariable>[] Samplers => _samplers;

    public ref ShaderBind<ShaderResourceVariable>[] this[ShaderBindType type] => ref _resources[(int)type];
}
