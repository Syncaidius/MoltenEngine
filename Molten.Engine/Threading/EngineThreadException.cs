namespace Molten.Threading
{
    public class EngineThreadException : Exception
    {
        public EngineThreadException(EngineThread thread, string name, string message) : base(message)
        {
            Thread = thread;
            ThreadName = name;
        }

        public EngineThread Thread { get; private set; }

        public string ThreadName { get; private set; }
    }
}
