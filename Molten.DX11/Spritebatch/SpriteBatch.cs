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
    public class SpriteBatch : EngineObject, ISpriteBatch
    {
        struct FlushData
        {
            public Material Material;
            public Material NoTexMaterial;
            public IShaderValue TextureValue;
            public IShaderValue TexSizeValue;
        }

        BufferSegment _segment;

        const int CLUSTER_EXPANSION = 5;
        const int SPRITE_EXPANSION = 100;
        const int ZONE_EXPANSION = 10;

        Stack<int> _clipStack;
        SpriteClipZone[] _clipZones;
        int _curClip;
        int _clipCount;
        int _clipsInitialized;

        SpriteCluster[] _clusterBank;
        int _clusterCount;

        int _spriteCapacity;
        int _vertexCount;

        int _drawnFrom;
        int _drawnTo;

        Material _defaultMaterial;
        Material _defaultNoTextureMaterial;

        internal SpriteBatch(RendererDX11 renderer, int spriteBufferSize = 2000)
        {

            _spriteCapacity = spriteBufferSize;
            _segment = renderer.DynamicVertexBuffer.Allocate<SpriteVertex>(_spriteCapacity);
            _segment.SetVertexFormat(typeof(SpriteVertex));

            _clusterBank = new SpriteCluster[CLUSTER_EXPANSION];
            _clipZones = new SpriteClipZone[ZONE_EXPANSION];

            for (int i = 0; i < _clusterBank.Length; i++)
                _clusterBank[i] = new SpriteCluster(SPRITE_EXPANSION);

            // Setup initial clip zone (which is disabled) ready for starting over.
            _clipStack = new Stack<int>();
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
            }
        }

        /// <summary>Disposes of the spritebatch.</summary>
        protected override void OnDispose()
        {
            _segment.Release();
        }

        private void ConfigureNewClip(Rectangle bounds, bool isActive)
        {
            int id = _clipCount;

            if (_clipZones.Length == id)
            {
                Array.Resize(ref _clipZones, _clipZones.Length + ZONE_EXPANSION);

                _clipZones[id] = new SpriteClipZone()
                {
                    Active = isActive,
                    ClusterIDs = new int[CLUSTER_EXPANSION],
                };

                _clipsInitialized++;
            }
            else
            {
                if (id == _clipsInitialized)
                {
                    _clipZones[id] = new SpriteClipZone()
                    {
                        Active = isActive,
                        ClusterIDs = new int[CLUSTER_EXPANSION],
                    };

                    _clipsInitialized++;
                }
                else
                {
                    _clipZones[id].ClusterCount = 0;
                    _clipZones[id].Active = isActive;
                }
            }

            _clipCount++;
            _curClip = id;
            _clipZones[_curClip].ClipBounds = bounds;
        }

        private SpriteCluster ConfigureNewCluster(SpriteClipZone clip, ITexture2D texture)
        {
            if (_clusterBank.Length == _clusterCount)
            {
                Array.Resize(ref _clusterBank, _clusterBank.Length + CLUSTER_EXPANSION);

                // Initialize new cluster slots.
                for (int i = _clusterCount; i < _clusterBank.Length; i++)
                    _clusterBank[i] = new SpriteCluster(SPRITE_EXPANSION);
            }

            SpriteCluster cluster = _clusterBank[_clusterCount];
            cluster.texture = texture;
            cluster.drawnTo = 0;
            cluster.drawnFrom = 0;
            cluster.spriteCount = 0;

            // Now assign the cluster to the current clip zone
            int clipCluster = clip.ClusterCount;

            // Resize zone's cluster ID array if needed
            if (clipCluster == clip.ClusterIDs.Length)
                Array.Resize(ref clip.ClusterIDs, clip.ClusterIDs.Length + CLUSTER_EXPANSION);

            clip.ClusterIDs[clipCluster] = _clusterCount;
            clip.ClusterCount++;
            _clusterCount++;

            return cluster;
        }

        /// <summary>Draws a string of text sprites by using a sprite font to source the needed data..</summary>
        /// <param name="font">The spritefont from which to retrieve font data.</param>
        /// <param name="text">The text to draw.</param>
        /// <param name="position">The position of the text.</param>
        /// <param name="color">The color of the text.</param>
        public void DrawString(ISpriteFont font, string text, Vector2 position, Color color)
        {
            SpriteClipZone clip = _clipZones[_curClip];
            SpriteCluster cluster;
            int sID = 0;

            // If the current cluster is for a different texture, start a new cluster.
            if (clip.ClusterCount == 0)
            {
                cluster = ConfigureNewCluster(clip, font.UnderlyingTexture);
            }
            else
            {
                int clusterID = clip.ClusterCount - 1;
                cluster = _clusterBank[clip.ClusterIDs[clusterID]];

                if (cluster.texture != font.UnderlyingTexture)
                    cluster = ConfigureNewCluster(clip, font.UnderlyingTexture);
                else
                    sID = cluster.spriteCount;
            }

            // Ensure the whole string can fit in the new/existing cluster.
            int sLength = cluster.sprites.Length;
            int needed = sID + text.Length;
            if (needed >= sLength)
                Array.Resize(ref cluster.sprites, sLength + SPRITE_EXPANSION + needed);

            //cycle through all characters in the string and process them
            int strLength = text == null ? 0 : text.Length;
            Rectangle invalid = Rectangle.Empty;
            Vector2 charPos = position;

            for (int i = 0; i < strLength; i++)
            {
                char c = text[i];
                Rectangle charRect = font.GetCharRect(c);

                // Set the sprite info
                cluster.sprites[sID++] = new SpriteVertex()
                {
                    Position = new Vector2(charPos.X, charPos.Y),
                    Size = new Vector2(charRect.Width, charRect.Height),
                    UV = new Vector4(charRect.X, charRect.Y, charRect.Right, charRect.Bottom),
                    Color = color,
                    Origin = Vector2.Zero,
                    Rotation = 0,
                };

                //increase pos by size of char (along X)
                charPos.X += charRect.Width;
            }

            cluster.spriteCount += text.Length;
        }

        public void Draw(Rectangle destination, Color color)
        {
            Draw(null, new Rectangle(0, 0, 1, 1), destination, color, 0, new Vector2());
        }

        /// <summary>Adds a sprite to the batch.</summary>
        /// <param name="destination">The rectangle defining the draw destination.</param>
        /// <param name="color">The color overlay/tiny of the sprite.</param>
        /// <param name="rotation">The rotation of the sprite, in radians.</param>
        /// <param name="origin">The origin, as a scale value. 1.0f will set the origin to the bottom-right corner of the sprite.
        /// 0.0f will set the origin to the top-left. </param>
        public void Draw(Rectangle destination, Color color, float rotation, Vector2 origin)
        {
            Draw(null, new Rectangle(0, 0, 1, 1), destination, color, rotation, origin);
        }

        /// <summary>Adds a sprite to the batch.</summary>
        /// <param name="texture"></param>
        /// <param name="destination"></param>
        /// <param name="color"></param>
        public void Draw(ITexture2D texture, Rectangle destination, Color color)
        {
            Rectangle src = new Rectangle(0, 0, texture.Width, texture.Height);
            Draw(texture, src, destination, color, 0, new Vector2());
        }

        /// <summary>Adds a sprite to the batch.</summary>
        /// <param name="texture"></param>
        /// <param name="destination"></param>
        /// <param name="color"></param>
        /// <param name="rotation"></param>
        /// <param name="origin"></param>
        public void Draw(ITexture2D texture, Rectangle destination, Color color, float rotation, Vector2 origin)
        {
            Rectangle src = new Rectangle(0, 0, texture.Width, texture.Height);
            Draw(texture, src, destination, color, rotation, origin);
        }

        /// <summary>Adds a sprite to the batch.</summary>
        /// <param name="texture"></param>
        /// <param name="position"></param>
        /// <param name="color"></param>
        public void Draw(ITexture2D texture, Vector2 position, Color color)
        {
            Rectangle src = new Rectangle(0, 0, texture.Width, texture.Height);
            Rectangle dest = new Rectangle((int)position.X, (int)position.Y, texture.Width, texture.Height);
            Draw(texture, src, dest, color, 0, new Vector2());
        }

        /// <summary>Adds a sprite to the batch.</summary>
        /// <param name="texture"></param>
        /// <param name="position"></param>
        /// <param name="color"></param>
        /// <param name="rotation"></param>
        /// <param name="origin"></param>
        public void Draw(ITexture2D texture, Vector2 position, Color color, float rotation, Vector2 origin)
        {
            Rectangle src = new Rectangle(0, 0, texture.Width, texture.Height);
            Rectangle dest = new Rectangle((int)position.X, (int)position.Y, texture.Width, texture.Height);
            Draw(texture, src, dest, color, rotation, origin);
        }

        /// <summary>Adds a sprite to the batch.</summary>
        /// <param name="texture"></param>
        /// <param name="position"></param>
        /// <param name="source"></param>
        /// <param name="color"></param>
        /// <param name="rotation">Rotation, in radians.</param>
        /// <param name="origin"></param>
        public void Draw(ITexture2D texture, Vector2 position, Rectangle source, Color color, float rotation, Vector2 scale, Vector2 origin)
        {
            Rectangle dest = new Rectangle()
            {
                X = (int)position.X,
                Y = (int)position.Y,
                Width = (int)(texture.Width * scale.X),
                Height = (int)(texture.Height * scale.Y),
            };
            Draw(texture, source, dest, color, rotation, origin);
        }

        /// <summary>Adds a sprite to the batch</summary>
        /// <param name="texture"></param>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="color"></param>
        /// <param name="rotation"></param>
        /// <param name="origin"></param>
        public void Draw(ITexture2D texture, Rectangle source, Rectangle destination, Color color, float rotation, Vector2 origin)
        {
            SpriteClipZone clip = _clipZones[_curClip];
            SpriteCluster cluster;
            int sID = 0;

            // If the current cluster is for a different texture, start a new cluster.
            if (clip.ClusterCount == 0)
            {
                cluster = ConfigureNewCluster(clip, texture);
            }
            else
            {
                int clusterID = clip.ClusterCount - 1;
                cluster = _clusterBank[clip.ClusterIDs[clusterID]];

                if (cluster.texture != texture)
                {
                    cluster = ConfigureNewCluster(clip, texture);
                }
                else
                {
                    sID = cluster.spriteCount;
                    int sLength = cluster.sprites.Length;

                    // If we can't fit the sprite into the cluster, expand.
                    if (sID == sLength)
                        Array.Resize(ref cluster.sprites, sLength + SPRITE_EXPANSION);
                }
            }

            // Set the sprite info
            cluster.sprites[sID] = new SpriteVertex()
            {
                Position = new Vector2(destination.X, destination.Y),
                Size = new Vector2(destination.Width, destination.Height),
                UV = new Vector4(source.X, source.Y, source.Right, source.Bottom),
                Color = color,
                Origin = origin,
                Rotation = rotation,
            };

            cluster.spriteCount++;
        }

        /// <summary>Pushes a new clipping zone into the sprite batch. All sprites <see cref="SpriteBatch.Draw"/> will be clipped
        /// to within the newly pushed bounds. Anything outside of it will be culled. Clipping bounds stack, so pushing another clipping
        /// bounds onto an existing one, will clip the second bounds to within the first/previous clipping bounds.</summary>
        /// <param name="clipBounds">The rectangle that represents the clipping zone or area.</param>
        public void PushClip(Rectangle clipBounds)
        {
            //constrain the clip area to inside of the previous clip, if one is active
            if (_clipZones[_curClip].Active)
            {
                Rectangle current = _clipZones[_curClip].ClipBounds;
                if (clipBounds.Left < current.Left)
                    clipBounds.Left = current.Left;
                if (clipBounds.Right > current.Right)
                    clipBounds.Right = current.Right;
                if (clipBounds.Top < current.Top)
                    clipBounds.Top = current.Top;
                if (clipBounds.Bottom > current.Bottom)
                    clipBounds.Bottom = current.Bottom;
            }

            //create new clip zone.
            _clipStack.Push(_curClip);
            ConfigureNewClip(clipBounds, true);
        }

        /// <summary>Pops a clip zone from the sprite batch.</summary>
        public void PopClip()
        {
            if (_clipStack.Count == 0)
                throw new InvalidOperationException("No clips to pop from the sprite batch. Use SpriteBatch.PushClip first.");

            // Create a new clip zone with the same bounds as the one that came before the popped zone
            int previous = _clipStack.Pop();
            ConfigureNewClip(_clipZones[previous].ClipBounds, true);
        }


        internal void Begin(Viewport viewBounds)
        {
            // Setup initial clip zone.
            ConfigureNewClip(viewBounds.Bounds, false);
        }

        /// <summary>Finalizes a batch of sprites, sorts them (if enabled) and then draws them.</summary>
        /// <param name="sortMode"></param>
        internal void Flush(GraphicsPipe pipe, ref Matrix viewProjection,
            BlendingPreset blend,
            DepthStencilPreset depth = DepthStencilPreset.ZDisabled,
            RasterizerPreset raster = RasterizerPreset.Default,
            Material material = null, Material noTextureMaterial = null)
        {
            //if nothing was added to the batch, don't bother with any draw operations.
            if (_clusterCount == 0)
                return;

            FlushData data = new FlushData()
            {
                Material = material ?? _defaultMaterial,
                NoTexMaterial = noTextureMaterial ?? _defaultNoTextureMaterial,
            };

            if (data.Material == null)
            {
                Reset();
                return;
            }

            data.TextureValue = data.Material["albedo"];
            data.TexSizeValue = data.Material["textureSize"];
            data.Material["worldViewProj"].Value = viewProjection;
            data.NoTexMaterial["worldViewProj"].Value = viewProjection;

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
                            int remaining = cluster.spriteCount - from;
                            int canFit = _spriteCapacity - _vertexCount;
                            int to = Math.Min(cluster.spriteCount, from + canFit);

                            //assign the start vertex to the cluster
                            cluster.startVertex = _vertexCount;

                            // Process until the end of the cluster, or until the buffer is full
                            int copyCount = to - from;
                            if (copyCount > 0)
                            {
                                stream.WriteRange(cluster.sprites, from, copyCount);
                                _vertexCount += copyCount;

                                //update cluster counters
                                cluster.drawnFrom = from;
                                cluster.drawnTo = to;
                            }
                            _drawnTo = clustersDone;


                            // Are we done?
                            if (cluster.drawnTo == cluster.spriteCount)
                                finishedDrawing = ++clustersDone == clip.ClusterCount;
                            else
                                break;

                        } while (!finishedDrawing);
                    });

                    FlushInternal(pipe, ref data, clip);
                } while (!finishedDrawing);
            }

            pipe.PopState();

            //reset counters
            Reset();
        }

        private void Reset()
        {
            //reset counters
            _clipStack.Clear();
            _curClip = 0;
            _clipCount = 0;
            _clusterCount = 0;
            _drawnFrom = 0;
            _drawnTo = 0;
        }

        private void FlushInternal(GraphicsPipe pipe, ref FlushData data, SpriteClipZone clip)
        {
            // Draw to the screen
            Material mat = null;

            for (int i = _drawnFrom; i <= _drawnTo; i++)
            {
                int cID = clip.ClusterIDs[i];

                if (_clusterBank[cID].texture != null)
                {
                    data.TexSizeValue.Value = new Vector2(_clusterBank[cID].texture.Width, _clusterBank[cID].texture.Height);
                    data.TextureValue.Value = _clusterBank[cID].texture;
                    mat = data.Material;
                }
                else
                {
                    mat = data.NoTexMaterial;
                }

                int startVertex = _clusterBank[cID].startVertex;
                int vertexCount = _clusterBank[cID].drawnTo - _clusterBank[cID].drawnFrom;
                pipe.Draw(mat, vertexCount, PrimitiveTopology.PointList, startVertex);
            }

            //reset all counters
            _vertexCount = 0;
            _drawnFrom = _drawnTo;
        }

        /// <summary>Gets the total number of clip zones currently in the batch.</summary>
        public int TotalClips { get { return _clipCount; } }
    }
}
