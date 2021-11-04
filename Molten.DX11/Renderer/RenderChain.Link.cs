using Molten.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    internal partial class RenderChain
    {
        internal class Link : IPoolable
        {
            List<Link> _previous = new List<Link>();
            List<Link> _next = new List<Link>();
            RenderStepBase _step;
            RenderChain _chain;
            bool _completed;

            internal Link(RenderChain chain)
            {
                _chain = chain;
            }

            public void Clear()
            {
                _previous.Clear();
                _next.Clear();
                _step = null;
                _completed = false;
            }

            internal void Set<T>() where T : RenderStepBase, new()
            {
                _step = _chain._renderer.GetRenderStep<T>();
            }

            internal Link Next<T>() where T : RenderStepBase, new()
            {
                Link next = _chain.LinkPool.GetInstance();
                next.Set<T>();
                _next.Add(next);
                next._previous.Add(this);

                return next;
            }

            /// <summary>
            /// Causes all of the current link's next links toconverge on the specified step type.
            /// </summary>
            /// <typeparam name="T">The type of step to converge at.</typeparam>
            /// <returns>The latest <see cref="Link"/> that was added as a result of the requested step being added.</returns>
            internal Link ConvergeAt<T>() where T : RenderStepBase, new()
            {
                Link next = _chain.LinkPool.GetInstance();
                next.Set<T>();
                ConvergeAt(this, next);
                return next;
            }

            private static void ConvergeAt(Link current, Link target)
            {
                if (current._next.Count > 0)
                {
                    for (int i = 0; i < current._next.Count; i++)
                        ConvergeAt(current._next[i], target);
                }
                else
                {
                    current._next.Add(target);
                    target._previous.Add(current);
                }
            }

            internal static void Recycle(Link link)
            {
                for (int i = 0; i < link._next.Count; i++)
                    Recycle(link._next[i]);

                link._chain.LinkPool.Recycle(link);
            }

            internal void Run(RendererDX11 renderer, RenderCamera camera, Context context, Timing time)
            {
                bool canStart = true;

                // Are the previous steps completed?
                do
                {
                    canStart = true;
                    for (int i = 0; i < _previous.Count; i++)
                        canStart = canStart && _previous[i]._completed;

                    // TODO do other things while the thread is waiting for the previous steps to complete?
                } while (canStart == false);

                // TODO update this once the renderer supports running render steps in deferred context threads.
                // TODO also consider spawning extra chain contexts so they can individually 
                _step.Render(renderer, camera, context, time);
                _completed = true;

                // Start the next steps
                for (int i = 0; i < _next.Count; i++)
                    _next[i].Run(renderer, camera, context, time);
            }
        }
    }
}
