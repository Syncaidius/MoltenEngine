using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Examples
{
    public abstract class MoltenExample
    {
        ContentLoadBatch _loader;

        public void Initialize(Engine engine)
        {
            OnInitialize(engine);

            _loader = engine.Content.GetLoadBatch();
            OnLoadContent(_loader);
            _loader.OnCompleted += _loader_OnCompleted;
            _loader.Dispatch();
        }

        private void _loader_OnCompleted(ContentLoadBatch loader)
        {
            // TODO hide loading screen and progress bar
        }

        protected abstract void OnInitialize(Engine engine);

        protected abstract void OnLoadContent(ContentLoadBatch loader);
    }
}
