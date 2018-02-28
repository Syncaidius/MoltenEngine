using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>
    /// A base class for sprite batcher implementations.
    /// </summary>
    public abstract class SpriteBatchBase : EngineObject
    {
        protected enum ClusterFormat
        {
            Sprite = 0, // Textured or untextured (rectangle) sprites

            Line = 1, // Untextured lines

            Triangle = 2, // Untextured triangles

            Circle = 3, // Untextured circles - Uses a geometry shader to handle this
        }

        protected class SpriteClipZone
        {
            public bool Active;
            public Rectangle ClipBounds;
            public int[] ClusterIDs;
            public int ClusterCount;
            public SpriteCluster CurCluster;
        }

        protected class SpriteCluster
        {
            public ClusterFormat Format;
            public ITexture2D Texture;
            public IMaterial Material;
            public SpriteVertex[] Sprites;
            public int SpriteCount;

            /// <summary>Number of sprites that have been drawn during the current draw call.</summary>
            public int drawnTo;
            public int drawnFrom;
            public int startVertex;

            internal SpriteCluster(int spriteCapacity)
            {
                Sprites = new SpriteVertex[spriteCapacity];
            }
        }

        const int CLUSTER_EXPANSION = 5;
        const int SPRITE_EXPANSION = 100;
        const int ZONE_EXPANSION = 10;

        protected Stack<int> _clipStack;
        protected SpriteClipZone[] _clipZones;
        protected int _curClip;
        protected int _clipCount;
        protected int _clipsInitialized;

        protected SpriteCluster[] _clusterBank;
        protected int _clusterCount;

        public SpriteBatchBase()
        {
            _clusterBank = new SpriteCluster[CLUSTER_EXPANSION];
            _clipZones = new SpriteClipZone[ZONE_EXPANSION];

            for (int i = 0; i < _clusterBank.Length; i++)
                _clusterBank[i] = new SpriteCluster(SPRITE_EXPANSION);

            // Setup initial clip zone (which is disabled) ready for starting over.
            _clipStack = new Stack<int>();
        }

        protected void ConfigureNewClip(Rectangle bounds, bool isActive)
        {
            int id = _clipCount;

            if (_clipZones.Length == id)
                Array.Resize(ref _clipZones, _clipZones.Length + ZONE_EXPANSION);

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
                _clipZones[id].CurCluster = null;
                _clipZones[id].Active = isActive;
            }

            _clipCount++;
            _curClip = id;
            _clipZones[_curClip].ClipBounds = bounds;
        }

        protected SpriteCluster ConfigureNewCluster(SpriteClipZone clip, ITexture2D texture, IMaterial material, ClusterFormat format)
        {
            if (_clusterBank.Length == _clusterCount)
            {
                Array.Resize(ref _clusterBank, _clusterBank.Length + CLUSTER_EXPANSION);

                // Initialize new cluster slots.
                for (int i = _clusterCount; i < _clusterBank.Length; i++)
                    _clusterBank[i] = new SpriteCluster(SPRITE_EXPANSION);
            }

            SpriteCluster cluster = _clusterBank[_clusterCount];
            cluster.Format = format;
            cluster.Texture = texture;
            cluster.Material = material;
            cluster.drawnTo = 0;
            cluster.drawnFrom = 0;
            cluster.SpriteCount = 0;

            // Now assign the cluster to the current clip zone
            int clipCluster = clip.ClusterCount;

            // Resize zone's cluster ID array if needed
            if (clipCluster == clip.ClusterIDs.Length)
                Array.Resize(ref clip.ClusterIDs, clip.ClusterIDs.Length + CLUSTER_EXPANSION);

            clip.CurCluster = cluster;
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
        public void DrawString(ISpriteFont font, string text, Vector2 position, Color color, IMaterial material = null)
        {
            SpriteClipZone clip = _clipZones[_curClip];
            int spriteID = 0;
            int strLength = text.Length;
            SpriteCluster cluster = GetCluster(clip, font.UnderlyingTexture, material, ClusterFormat.Sprite, strLength, out spriteID);

            // Cycle through all characters in the string and process them
            Rectangle invalid = Rectangle.Empty;
            Vector2 charPos = position;
            for (int i = 0; i < strLength; i++)
            {
                char c = text[i];
                Rectangle charRect = font.GetCharRect(c);

                // Set the sprite info
                cluster.Sprites[spriteID++] = new SpriteVertex()
                {
                    Position = new Vector2(charPos.X, charPos.Y),
                    Size = new Vector2(charRect.Width, charRect.Height),
                    UV = new Vector4(charRect.X, charRect.Y, charRect.Right, charRect.Bottom),
                    Color = color,
                    Origin = Vector2.Zero,
                    Rotation = 0,
                };

                // Increase pos by size of char (along X)
                charPos.X += charRect.Width;
            }

            cluster.SpriteCount += strLength;
        }

        public void DrawCircle(Vector2 center, float radius, float startAngle, float endAngle, Color col, int segments = 16)
        {
            DrawEllipse(center, radius, radius, startAngle, endAngle, col, segments);
        }

        public void DrawCircle(Vector2 center, float radius, Color col, int segments = 16)
        {
            DrawEllipse(center, radius, radius, 0 * MathHelper.DegToRad, 360 * MathHelper.DegToRad, col, segments);
        }

        public void DrawEllipse(Vector2 center, float xRadius, float yRadius, Color col, int segments = 16)
        {
            DrawEllipse(center, xRadius, yRadius, 0 * MathHelper.DegToRad, 360 * MathHelper.DegToRad, col, segments);
        }

        public void DrawEllipse(Vector2 center, float xRadius, float yRadius, float startAngle, float endAngle, Color col, int segments = 16)
        {
            throw new NotImplementedException();
        }

        public void DrawTriangle(Vector2 p1, Vector2 p2, Vector2 p3, Color col)
        {
            throw new NotImplementedException();
        }

        public void DrawTriangle(Vector2 p1, Vector2 p2, Vector2 p3, Color color1, Color color2, Color color3)
        {
            throw new NotImplementedException();
        }

        public void DrawPolygon(IList<Vector2> points, IList<Color> colors)
        {
            throw new NotImplementedException();
        }

        public void DrawPolygon(IList<Vector2> points, Color color)
        {
            throw new NotImplementedException();
        }

        public void DrawLines(IList<Vector2> points, IList<Color> pointColors, float thickness)
        {
            throw new NotImplementedException();
        }

        public void DrawLines(IList<Vector2> points, Color color, float thickness)
        {
            if (points.Count == 2)
            {
                DrawLine(points[0], points[1], color, color, thickness);
            }
            else
            {
                SpriteClipZone clip = _clipZones[_curClip];
                int spriteID = 0;
                int lineCount = points.Count - 1;
                SpriteCluster cluster = GetCluster(clip, null, null, ClusterFormat.Line, lineCount, out spriteID);

                Vector2 p1, p2;
                int last = points.Count - 1;
                int prev = 0;
                int next = 1;

                for (int i = 0; i < last; i++)
                {
                    p1 = points[i];
                    p2 = points[next];

                    // Pack two points into a single sprite vertex. This equates to 1 vertex per line.
                    cluster.Sprites[spriteID] = new SpriteVertex()
                    {
                        Position = p1,
                        Size = p2,
                        UV = color.ToColor4(),
                        Color = color,
                        Rotation = thickness,
                    };

                    // Provide the previous line with the direction of the current line.
                    if (prev < i)
                        cluster.Sprites[spriteID - 1].Origin = p2 - p1;

                    // If there is no line after the current, use the current line's direction to fill the tangent calculation.
                    if (next + 1 == last)
                        cluster.Sprites[spriteID].Origin = p2 - p1;

                    spriteID++;
                    next++;
                }

                cluster.SpriteCount += lineCount;
            }
        }

        public void DrawLine(Vector2 p1, Vector2 p2, Color col, float thickness)
        {
            DrawLine(p1, p2, col, col, thickness);
        }

        public void DrawLine(Vector2 p1, Vector2 p2, Color col1, Color col2, float thickness)
        {
            SpriteClipZone clip = _clipZones[_curClip];
            int spriteID = 0;
            SpriteCluster cluster = GetCluster(clip, null, null, ClusterFormat.Line, 1, out spriteID);

            // Pack two line points into a single sprite vertex.
            cluster.Sprites[spriteID++] = new SpriteVertex()
            {
                Position = p1,
                Size = p2,
                UV = col1.ToColor4(), 
                Color = col2,
                
                // Normal of next line. This is used for creating sharp edges when drawing multiple lines. 
                // In this case, we set it to the direction of the current line, because we're just drawing one.
                Origin = p2 - p1, 
                Rotation = thickness,
            };

            cluster.SpriteCount++;
        }

        public void DrawRect(Rectangle destination, Color color, IMaterial material = null)
        {
            Draw(null, new Rectangle(0, 0, 1, 1), destination, color, 0, new Vector2(), material);
        }

        /// <summary>Adds a sprite to the batch.</summary>
        /// <param name="destination">The rectangle defining the draw destination.</param>
        /// <param name="color">The color overlay/tiny of the sprite.</param>
        /// <param name="rotation">The rotation of the sprite, in radians.</param>
        /// <param name="origin">The origin, as a scale value. 1.0f will set the origin to the bottom-right corner of the sprite.
        /// 0.0f will set the origin to the top-left. </param>
        public void DrawRect(Rectangle destination, Color color, float rotation, Vector2 origin, IMaterial material = null)
        {
            Draw(null, new Rectangle(0, 0, 1, 1), destination, color, rotation, origin, material);
        }

        /// <summary>Adds a sprite to the batch.</summary>
        /// <param name="texture"></param>
        /// <param name="destination"></param>
        /// <param name="color"></param>
        public void Draw(ITexture2D texture, Rectangle destination, Color color, IMaterial material = null)
        {
            Rectangle src = new Rectangle(0, 0, texture.Width, texture.Height);
            Draw(texture, src, destination, color, 0, new Vector2(), material);
        }

        /// <summary>Adds a sprite to the batch.</summary>
        /// <param name="texture"></param>
        /// <param name="destination"></param>
        /// <param name="color"></param>
        /// <param name="rotation"></param>
        /// <param name="origin"></param>
        public void Draw(ITexture2D texture, Rectangle destination, Color color, float rotation, Vector2 origin, IMaterial material = null)
        {
            Rectangle src = new Rectangle(0, 0, texture.Width, texture.Height);
            Draw(texture, src, destination, color, rotation, origin, material);
        }

        /// <summary>Adds a sprite to the batch.</summary>
        /// <param name="texture"></param>
        /// <param name="position"></param>
        /// <param name="color"></param>
        public void Draw(ITexture2D texture, Vector2 position, Color color, IMaterial material = null)
        {
            Rectangle src = new Rectangle(0, 0, texture.Width, texture.Height);
            Rectangle dest = new Rectangle((int)position.X, (int)position.Y, texture.Width, texture.Height);
            Draw(texture, src, dest, color, 0, new Vector2(), material);
        }

        /// <summary>Adds a sprite to the batch.</summary>
        /// <param name="texture"></param>
        /// <param name="position"></param>
        /// <param name="color"></param>
        /// <param name="rotation"></param>
        /// <param name="origin"></param>
        public void Draw(ITexture2D texture, Vector2 position, Color color, float rotation, Vector2 origin, IMaterial material = null)
        {
            Rectangle src = new Rectangle(0, 0, texture.Width, texture.Height);
            Rectangle dest = new Rectangle((int)position.X, (int)position.Y, texture.Width, texture.Height);
            Draw(texture, src, dest, color, rotation, origin, material);
        }

        /// <summary>Adds a sprite to the batch.</summary>
        /// <param name="texture"></param>
        /// <param name="position"></param>
        /// <param name="source"></param>
        /// <param name="color"></param>
        /// <param name="rotation">Rotation, in radians.</param>
        /// <param name="origin"></param>
        public void Draw(ITexture2D texture, Vector2 position, Rectangle source, Color color, float rotation, Vector2 scale, Vector2 origin, IMaterial material = null)
        {
            Rectangle dest = new Rectangle()
            {
                X = (int)position.X,
                Y = (int)position.Y,
                Width = (int)(texture.Width * scale.X),
                Height = (int)(texture.Height * scale.Y),
            };
            Draw(texture, source, dest, color, rotation, origin, material);
        }

        /// <summary>Adds a sprite to the batch</summary>
        /// <param name="texture"></param>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="color"></param>
        /// <param name="rotation"></param>
        /// <param name="origin"></param>
        public void Draw(ITexture2D texture, Rectangle source, Rectangle destination, Color color, float rotation, Vector2 origin, IMaterial material)
        {
            int spriteID = 0;
            SpriteCluster cluster = GetCluster(_clipZones[_curClip], texture, material, ClusterFormat.Sprite, 1, out spriteID);

            // Set the sprite info
            cluster.Sprites[spriteID] = new SpriteVertex()
            {
                Position = new Vector2(destination.X, destination.Y),
                Size = new Vector2(destination.Width, destination.Height),
                UV = new Vector4(source.X, source.Y, source.Right, source.Bottom),
                Color = color,
                Origin = origin,
                Rotation = rotation,
            };

            cluster.SpriteCount++;
        }

        public void Draw(SpriteBatchCache cache)
        {
            if (cache._clusterCount == 0)
                return;

            SpriteClipZone clip = _clipZones[_curClip];
            SpriteCluster cluster;
            SpriteClipZone cacheClip;
            SpriteCluster cacheCluster;

            for (int i = 0; i < cache._clipCount; i++)
            {
                cacheClip = cache._clipZones[i];

                // Configure a new clip if the cache's current clip is active.
                if (cacheClip.Active)
                {
                    ConfigureNewClip(cacheClip.ClipBounds, true);
                    clip = _clipZones[_curClip];
                }

                // If the current cache clip is empty, continue to the next
                if (cacheClip.ClusterCount == 0)
                    continue;

                int cacheClusterID = 0;
                cacheCluster = cache._clusterBank[cacheClip.ClusterIDs[cacheClusterID]];
                if (clip.ClusterCount == 0)
                {
                    // Prepare a new cluster to match the current cache cluster.
                    cluster = ConfigureNewCluster(clip, cacheCluster.Texture, cacheCluster.Material, cacheCluster.Format);
                }
                else
                {
                    cluster = clip.CurCluster;
                    // See if we can merge the first cache cluster into the current. If not, create a new cluster to start copying to.
                    if (cluster.Texture != cacheCluster.Texture || cluster.Material != cacheCluster.Material || cluster.Format != cacheCluster.Format)
                        cluster = ConfigureNewCluster(clip, cacheCluster.Texture, cacheCluster.Material, cacheCluster.Format);
                }


                // Copy cache clusters
                while (cacheCluster != null)
                {
                    int needed = cluster.SpriteCount + cacheCluster.SpriteCount;
                    if (needed >= cluster.Sprites.Length)
                        Array.Resize(ref cluster.Sprites, needed + SPRITE_EXPANSION);

                    Array.Copy(cacheCluster.Sprites, 0, cluster.Sprites, cluster.SpriteCount, cacheCluster.SpriteCount);
                    cacheClusterID++;
                    cluster.SpriteCount += cacheCluster.SpriteCount;

                    // Prepare the next cache cluster copy, if needed.
                    if (cacheClusterID < cacheClip.ClusterCount)
                    { 
                        cacheCluster = cache._clusterBank[cacheClip.ClusterIDs[cacheClusterID]];
                        cluster = ConfigureNewCluster(clip, cacheCluster.Texture, cacheCluster.Material, cacheCluster.Format);
                    }
                    else
                    {
                        cacheCluster = null;
                    }
                }
            }
        }

        protected SpriteCluster GetCluster(SpriteClipZone clip, ITexture2D texture, IMaterial material, ClusterFormat format, int needed, out int spriteID)
        {
            SpriteCluster cluster;
            spriteID = 0;

            // If the current cluster is for a different texture, start a new cluster.
            if (clip.ClusterCount == 0)
            {
                cluster = ConfigureNewCluster(clip, texture, material, format);
            }
            else
            {
                cluster = clip.CurCluster;
                if (cluster.Texture != texture || cluster.Material != material || cluster.Format != format)
                {
                    cluster = ConfigureNewCluster(clip, texture, material, format);
                }
                else
                {
                    spriteID = cluster.SpriteCount;
                    needed += spriteID;
                    int sLength = cluster.Sprites.Length;

                    // If we can't fit the sprite into the cluster, expand.
                    if (needed >= sLength)
                        Array.Resize(ref cluster.Sprites, sLength + SPRITE_EXPANSION + needed);
                }
            }

            return cluster;
        }

        /// <summary>Pushes a new clipping zone into the sprite batch. All sprites <see cref="SpriteBatch.Draw"/> will be clipped
        /// to within the newly pushed bounds. Anything outside of it will be culled. Clipping bounds stack, so pushing another clipping
        /// bounds onto an existing one, will clip the second bounds to within the first/previous clipping bounds.</summary>
        /// <param name="clipBounds">The rectangle that represents the clipping zone or area.</param>
        public void PushClip(Rectangle clipBounds)
        {
            // Constrain the clip area to inside of the previous clip, if one is active
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

            // Create new clip zone.
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

        protected virtual void Reset()
        {
            //reset counters
            _clipStack.Clear();
            _curClip = 0;
            _clipCount = 0;
            _clusterCount = 0;
        }

        /// <summary>Gets the total number of clip zones currently in the batch.</summary>
        public int ClipDepth { get { return _clipCount; } }
    }
}
