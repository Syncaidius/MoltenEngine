using Molten.Threading;

namespace Molten.Services
{
    public class ServiceStartupProperties
    {
        public EngineService Instance { get; init; }

        public ThreadingMode ThreadMode { get; init; }
    }
}
