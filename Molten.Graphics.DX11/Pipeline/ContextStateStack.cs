namespace Molten.Graphics
{
    internal class ContextStateStack
    {
        const int STACK_INCREMENT = 5;
        GraphicsState[] _stack;
        int _stackSize = 0;
        CommandQueueDX11 _context;

        internal ContextStateStack(CommandQueueDX11 context)
        {
            _stack = new GraphicsState[STACK_INCREMENT];
            for (int i = 0; i < _stack.Length; i++)
                _stack[i] = new GraphicsState(context);

            _context = context;
        }

        /// <summary>Pushes the current state of a pipe onto the stack.</summary>
        /// <returns></returns>
        public int Push()
        {
            int id = _stackSize++;

            if (_stackSize >= _stack.Length)
            {
                Array.Resize(ref _stack, _stack.Length + STACK_INCREMENT);

                // Initialize new additions
                for (int i = _stackSize; i < _stack.Length; i++)
                    _stack[i] = new GraphicsState(_context);
            }

            _stack[id].Capture();

            return id;
        }

        /// <summary>Pops the last saved state off the stack and restores it on its parent pipe.</summary>
        public void Pop()
        {
            _stackSize--;
            _stack[_stackSize].Restore();
            _stack[_stackSize].Clear();
        }

        public void PopTo(int id)
        {
            // Restore the expected state.
            _stack[id].Restore();

            // Clear the ones ahead of the expected state.
            while (_stackSize > id)
            {
                _stackSize--;
                _stack[_stackSize].Clear();
            }
        }

        internal GraphicsState FirstState => _stackSize > 0 ? _stack[0] : null;

        internal GraphicsState LastState
        {
            get
            {
                int last = _stackSize - 1;
                return last < 0 ? null : _stack[last];
            }
        }
    }
}
