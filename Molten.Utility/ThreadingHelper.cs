using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Molten
{
    public static class ThreadingHelper
    {
        public static void InterlockSpinWait(ref int lockingVal, Action callback)
        {
            SpinWait wait = new SpinWait();
            while (true)
            {
                if(0 == Interlocked.Exchange(ref lockingVal, 1))
                {
                    callback();
                    Interlocked.Exchange(ref lockingVal, 0);
                    return;
                }
                wait.SpinOnce();
            }
        }
    }
}
