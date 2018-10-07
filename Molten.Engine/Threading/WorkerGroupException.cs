using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Threading
{
    public class WorkerGroupException : Exception
    {
        public WorkerGroupException(WorkerGroup group, string name, string message) : base(message)
        {
            Group = group;
            GroupName = name;
        }

        public WorkerGroup Group { get; private set; }

        public string GroupName { get; private set; }
    }
}
