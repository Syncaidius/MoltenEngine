using System.Collections;

namespace Molten.Collections
{
    public partial class ThreadedQueue<T>
    {
        public class Enumerator : IEnumerator<T>
        {
            ThreadedQueue<T> _queue;
            int _position = -1;
            int _start = -1;
            int _next = -1;

            public Enumerator(ThreadedQueue<T> queue)
            {
                _queue = queue;
                _start = _queue._queueStart;
                _next = _queue._next;

                Reset();
            }
            public T Current => _queue._items[_position];

            object IEnumerator.Current => _queue._items[_position];

            public void Dispose()
            {
                _queue = null;
            }

            public bool MoveNext()
            {
                if ((_start != _queue._queueStart) || (_next != _queue._next))
                    throw new InvalidOperationException("Collection was modified");

                if (_position != _queue._next)
                {
                    if (_position == _queue._items.Length)
                        _position = 0;
                    else
                        _position++;

                    return (_position != _queue._next);
                }
                else
                {
                    return false;
                }
            }

            public void Reset()
            {
                if ((_start != _queue._queueStart) || (_next != _queue._next))
                    throw new InvalidOperationException("Collection was modified");

                _position = _start - 1;
            }
        }
    }
}
