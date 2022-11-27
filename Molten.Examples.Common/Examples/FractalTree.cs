using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Molten.Graphics;
using Molten.Input;

namespace Molten.Examples
{
    [Example("Fractal Tree", "Demostrates how to draw a fractal tree using SpriteBatcher.")]
    public class FractalTree : MoltenExample
    {
        class FractalBranch
        {
            public Vector2F Start;

            public Vector2F End;

            public float Length { get; }

            public FractalBranch(Vector2F pos, Vector2F dir, float len)
            {
                Start = pos;
                Length = len;
                End = pos + (dir * len);
            }

            public List<FractalBranch> Branches { get; } = new List<FractalBranch>();

            public void Branch(Random rng, int branchCount, float scale, float arcRadians)
            {
                if (Branches.Count > 0)
                    return;

                Vector2F dir = End - Start;
                dir.Normalize();

                // ang will be the mid-point of our new arc
                float ang = MathF.Atan2(-dir.Y, dir.X);
                ang += arcRadians / 2;
                float angInc = arcRadians;

                for(int i = 0; i < branchCount; i++)
                {
                    ang += angInc;
                    Vector2F bDir = new Vector2F()
                    {
                        X = MathF.Sin(ang),
                        Y = MathF.Cos(ang)
                    };

                    Branches.Add(new FractalBranch(End, bDir, (int)(Length * rng.NextDouble())));
                }
            }

            public void Draw(SpriteBatcher sb, float thickness, Color color)
            {
                sb.DrawLine(Start, End, color, thickness);
                foreach (FractalBranch b in Branches)
                    b.Draw(sb, thickness, color);
            }
        }

        FractalBranch _rootBranch;

        protected override void OnLoadContent(ContentLoadBatch loader)
        {
            base.OnLoadContent(loader);
            loader.OnCompleted += Loader_OnCompleted;
        }

        private void SpawnFractalTree(FractalBranch branch, int branchCount, int maxDepth, int depth = 0, float arc = float.Pi / 4, float scale = 0.80f)
        {
            if (depth == maxDepth)
                return;

            branch.Branch(Rng, branchCount, scale, arc);

            foreach (FractalBranch b in branch.Branches)
                SpawnFractalTree(b, branchCount, maxDepth, depth + 1, arc, scale);
        }

        private void Loader_OnCompleted(ContentLoadBatch loader)
        {
            // Spawn fractal tree
            _rootBranch = new FractalBranch(new Vector2F(700, 900), new Vector2F(0, -1), 180);
            SpawnFractalTree(_rootBranch, 2, 12, 0, float.Pi / 4f);

            SampleSpriteRenderComponent com = SpriteLayer.AddObjectWithComponent<SampleSpriteRenderComponent>();
            com.RenderCallback = (sb) =>
            {
                _rootBranch.Draw(sb, 1, Color.White);
            };
        }

        protected override void OnUpdate(Timing time)
        {
            base.OnUpdate(time);

        }
    }
}
