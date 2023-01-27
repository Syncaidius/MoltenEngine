﻿namespace Molten.Graphics
{
    /// <summary>
    /// Represents an implementation of a shader, or part of a shader, such as a material pass.
    /// </summary>
    public interface IShaderElement : IDisposable
    {
        /// <summary>
        /// Gets the name of the shader element.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets or sets the number of iterations the shader/component should be run.
        /// </summary>
        int Iterations { get; set; }
    }
}
