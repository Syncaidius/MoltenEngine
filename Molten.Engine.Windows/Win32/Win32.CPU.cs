namespace Molten.Windows32;

public class Win32CPU
{
    internal Win32CPU() { }

    /// <summary>Gets the friendly name of the processor the application is currently running on.</summary>
    /// <returns></returns>
    public string GetName()
    {
        return Win32.GetValue<string>("Win32_Processor", "Name");
    }

    /// <summary>Returns the speed of the current processor, in Mhz</summary>
    /// <returns></returns>
    public uint GetSpeed()
    {
        return Win32.GetValue<uint>("Win32_Processor", "MaxClockSpeed");
    }

    /// <summary>Gets the number of processor cores.</summary>
    /// <returns></returns>
    public uint GetCoreCount()
    {
        return Win32.GetValue<uint>("Win32_Processor", "NumberOfCores");
    }

    /// <summary>Gets the number of hardware threads.</summary>
    /// <returns></returns>
    public uint GetThreadCount()
    {
        return Win32.GetValue<uint>("Win32_Processor", "ThreadCount");
    }
}
