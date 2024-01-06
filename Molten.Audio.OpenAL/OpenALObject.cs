using Silk.NET.OpenAL;

namespace Molten.Audio.OpenAL;

/// <summary>
/// Represents a
/// </summary>
/// <typeparam name="E">Error type</typeparam>
public abstract class OpenALObject : EngineObject
{

    internal OpenALObject(AudioServiceAL service)
    {
        Service = service;
    }

    /// <summary>
    /// Gets the OpenAL error code for the current <see cref="OpenALObject{E}"/>
    /// </summary>
    protected internal bool HasError { get; protected set; }

    /// <summary>
    /// Checks if an Open AL audio error occurred. Returns true if there was an error, false if no error.
    /// </summary>
    /// <param name="errorMessage">The error message to include in the log entry, if an error is detected.</param>
    /// <returns>A boolean value. True for error. False for no error.</returns>
    internal bool CheckAlError(string errorMessage)
    {
        // Don't make any calls if we already have an error
        if (HasError)
            return true;

        AudioError alError = Service.Al.GetError();
        if (alError != AudioError.NoError)
        {
            HasError = true;

            string codeMsg = "";
            switch (alError)
            {
                default: codeMsg = alError.ToString(); break;
            }

            Service.Log.Error($"[AL] {errorMessage ?? "Error"}: {codeMsg}");
            return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if an Open AL audio error occurred. Returns true if there was an error, false if no error.
    /// </summary>
    /// <returns>A boolean value. True for error. False for no error.</returns>
    internal unsafe bool CheckAlError()
    {
        return Service.Al.GetError() != AudioError.NoError;
    }

    /// <summary>
    /// Checks if an Open AL Context error occurred. Returns true if there was an error, false if no error.
    /// </summary>
    /// <param name="errorMessage">The error message to include in the log entry, if an error is detected.</param>
    /// <returns>A boolean value. True for error. False for no error.</returns>
    internal unsafe bool CheckAlcError(string errorMessage, Device* device = null)
    {
        if (HasError)
            return true;

        if (device == null)
            device = Service.ActiveOutput.Ptr;

        ContextError alError = Service.Alc.GetError(device);
        if (alError != ContextError.NoError)
        {
            HasError = true;
            string codeMsg = "";
            switch (alError)
            {
                default: codeMsg = alError.ToString(); break;
            }

            Service.Log.Error($"[ALC] {errorMessage ?? "Error"}: {codeMsg}");
            return true;
        }

        return false;
    }

    /// <summary>
    /// Checks if an Open AL Context error occurred. Returns true if there was an error, false if no error.
    /// </summary>
    /// <returns>A boolean value. True for error. False for no error.</returns>
    internal unsafe bool CheckAlcError()
    {
        return Service.Alc.GetError(Service.ActiveOutput.Ptr) != ContextError.NoError;
    }

    /// <summary>
    /// Gets the <see cref="AudioServiceAL"/> that owns the current <see cref="OpenALObject"/>.
    /// </summary>
    internal AudioServiceAL Service { get; }
}
