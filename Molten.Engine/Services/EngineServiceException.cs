namespace Molten;

public class EngineServiceException : Exception
{
    public EngineServiceException(EngineService service, string message) : base(message)
    {
        Service = service;
    }

    public EngineService Service { get; }
}
