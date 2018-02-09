using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten
{
    public class ContentResult
    {
        internal Dictionary<Type, List<object>> Objects = new Dictionary<Type, List<object>>();

        internal void Clear()
        {
            Objects.Clear();
        }

        public void AddResult<T>(T obj)
        {
            if (obj == null)
                return;

            Type t = typeof(T);
            AddResult(t, obj);
        }

        public void AddResult(Type t, object obj)
        {
            List<object> group = null;
            if (!Objects.TryGetValue(t, out group))
            {
                group = new List<object>();
                Objects.Add(t, group);
            }

            group.Add(obj);
        }
    }
}
