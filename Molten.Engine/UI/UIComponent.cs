using Molten.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.UI
{
    public abstract class UIComponent : IRenderable2D, ISceneObject
    {
        public Scene Scene { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Update(Timing time)
        {
            throw new NotImplementedException();
        }

        public void Render(SpriteBatch sb)
        {
            throw new NotImplementedException();
        }
    }
}
