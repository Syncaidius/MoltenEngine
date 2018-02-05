using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Collections
{
    public partial class ThreadedList<T>
    {
        public class Enumerator : IEnumerator<T>
        {
            ThreadedList<T> _list;
            int _position = -1;
            int _startVersion = -1;

            public Enumerator(ThreadedList<T> list)
            {
                _list = list;
                _startVersion = list._version;
                Reset();
            }
            public T Current
            {
                get { return _list._items[_position]; }
            }

            object IEnumerator.Current
            {
                get { return _list._items[_position]; }
            }

            public void Dispose()
            {
                _list = null;
            }

            public bool MoveNext()
            {
                if ((_startVersion != _list._version))
                    throw new InvalidOperationException("Collection was modified");
                _position++;
                return (_position < _list._count);
            }

            public void Reset()
            {
                if ((_startVersion != _list._version))
                    throw new InvalidOperationException("Collection was modified");
                _position = -1;
            }
        }
    }
}
