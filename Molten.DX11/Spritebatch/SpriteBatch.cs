using SharpDX.Direct3D;
using Molten.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace Molten.Graphics
{
    public class SpriteBatch : SpriteBatchBase, ISpriteBatch
    {
        BufferSegment _segment;
        int _vertexCount;
        int _drawnFrom;
        int _drawnTo;
        int _spriteCapacity;

        Material _defaultMaterial;
        Material _defaultNoTextureMaterial;

        IShaderValue _valDefaultAlbedo;
        IShaderValue _valDefaultWvp;
        IShaderValue _valDefaultTexSize;
        IShaderValue _valNoTexWvp;
        Matrix _viewProjection;


        internal SpriteBatch(RendererDX11 renderer, int spriteBufferSize = 2000)
        {
            _spriteCapacity = spriteBufferSize;
            _segment = renderer.DynamicVertexBuffer.Allocate<SpriteVertex>(_spriteCapacity);
            _segment.SetVertexFormat(typeof(SpriteVertex));

            string source = null;
            string namepace = "Molten.Graphics.Assets.sprite.sbm";
            using (Stream stream = EmbeddedResourceReader.GetStream(namepace, typeof(RendererDX11).Assembly))
            {
                using (StreamReader reader = new StreamReader(stream))
                    source = reader.ReadToEnd();
            }

            if (!string.IsNullOrWhiteSpace(source))
            {
                ShaderParseResult result = renderer.ShaderCompiler.Parse(source, namepace);
                _defaultMaterial = result["material", "sprite-texture"] as Material;
                _defaultNoTextureMaterial = result["material", "sprite-no-texture"] as Material;

                _valDefaultAlbedo = _defaultMaterial["albedo"];
                _valDefaultTexSize = _defaultMaterial["textureSize"];
                _valDefaultWvp = _defaultMaterial["worldViewProj"];
                _valNoTexWvp = _defaultNoTextureMaterial["worldViewProj"];
            }
        }

        /// <summary>Disposes of the spritebatch.</summary>
        protected override void OnDispose()
        {
            _segment.Release();
        }

        internal void Begin(Viewport viewBounds)
        {
            ConfigureNewClip(viewBounds.Bounds, false); // Initial clip zone
        }

        /// <summary>Finalizes a batch of sprites, sorts them (if enabled) and then draws them.</summary>
        /// <param name="sortMode"></param>
        internal void Flush(GraphicsPipe pipe, ref Matrix viewProjection,
            BlendingPreset blend,
            DepthStencilPreset depth = DepthStencilPreset.ZDisabled,
            RasterizerPreset raster = RasterizerPreset.Default)
        {
            //if nothing was added to the batch, don't bother with any draw operations.
            if (_clusterCount == 0)
                return;

            _viewProjection = viewProjection;

            // Swap the default depth mode for Sprite2D mode, if attemping to use .Default as it is no good for rendering sprites efficiently.
            DepthStencilPreset depthPreset = depth == DepthStencilPreset.Default ? DepthStencilPreset.Sprite2D : depth;

            pipe.PushState();
            pipe.DepthStencil.SetPreset(depthPreset);
            pipe.BlendState.SetPreset(blend);
            pipe.SetVertexSegment(_segment, 0);

            // Run through all clip zones
            SpriteClipZone clip;
            for (int c = 0; c < _clipCount; c++)
            {
                clip = _clipZones[c];

                //reset to-from counters.
                _drawnFrom = 0;
                _drawnTo = 0;

                // Set rasterizer state + scissor rect
                pipe.Rasterizer.SetPreset(clip.Active ? RasterizerPreset.ScissorTest : raster);
                pipe.Rasterizer.SetScissorRectangle(clip.ClipBounds);

                //// Flush cluster within current clip-zone.
                int clustersDone = 0;
                bool finishedDrawing = false;
                do
                {
                    _segment.Map(pipe, (buffer, stream) =>
                    {
                        SpriteCluster cluster;
                        do
                        {
                            int cID = clip.ClusterIDs[clustersDone];
                            cluster = _clusterBank[cID];

                            int from = cluster.drawnTo;
                            int remaining = cluster.SpriteCount - from;
                            int canFit = _spriteCapacity - _vertexCount;
                            int to = Math.Min(cluster.SpriteCount, from + canFit);

                            //assign the start vertex to the cluster
                            cluster.startVertex = _vertexCount;

                            // Process until the end of the cluster, or until the buffer is full
                            int copyCount = to - from;
                            if (copyCount > 0)
                            {
                                stream.WriteRange(cluster.Sprites, from, copyCount);
                                _vertexCount += copyCount;

                                //update cluster counters
                                cluster.drawnFrom = from;
                                cluster.drawnTo = to;
                            }
                            _drawnTo = clustersDone;


                            // Are we done?
                            if (cluster.drawnTo == cluster.SpriteCount)
                                finishedDrawing = ++clustersDone == clip.ClusterCount;
                            else
                                break;

                        } while (!finishedDrawing);
                    });

                    FlushInternal(pipe, clip);
                } while (!finishedDrawing);
            }

            pipe.PopState();

            //reset counters
            Reset();
        }

        private void FlushInternal(GraphicsPipe pipe, SpriteClipZone clip)
        {
            // Draw to the screen
            Material mat = null;
            for (int i = _drawnFrom; i <= _drawnTo; i++)
            {
                int cID = clip.ClusterIDs[i];
                mat = _clusterBank[cID].Material as Material;

                if (_clusterBank[cID].Texture != null)
                {
                    Vector2 texSize = new Vector2(_clusterBank[cID].Texture.Width, _clusterBank[cID].Texture.Height);
                    if (mat != null)
                    {
                        // TODO improve this to avoid dictionary lookups.
                        mat["worldViewProj"].Value = _viewProjection;
                        mat["albedo"].Value = _clusterBank[cID].Texture;
                        mat["textureSize"].Value = texSize;
                    }
                    else
                    {
                        mat = _defaultMaterial;
                        _valDefaultWvp.Value = _viewProjection;
                        _valDefaultTexSize.Value = texSize;
                        _valDefaultAlbedo.Value = _clusterBank[cID].Texture;
                    }
                }
                else
                {
                    if (mat != null)
                    {
                        mat["worldViewProj"].Value = _viewProjection;
                    }
                    else
                    {
                        mat = _defaultNoTextureMaterial;
                        _valNoTexWvp.Value = _viewProjection;
                    }
                }

                int startVertex = _clusterBank[cID].startVertex;
                int vertexCount = _clusterBank[cID].drawnTo - _clusterBank[cID].drawnFrom;
                pipe.Draw(mat, vertexCount, PrimitiveTopology.PointList, startVertex);
            }

            //reset all counters
            _vertexCount = 0;
            _drawnFrom = _drawnTo;
        }

        protected override void Reset()
        {
            base.Reset();
            _drawnFrom = 0;
            _drawnTo = 0;
        }
    }
}
