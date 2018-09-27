using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Molten.Graphics
{
    /// <summary>
    /// A base class for sprite batcher implementations.
    /// </summary>
    public abstract class SpriteBatch : EngineObject
    {
        // This is must match [maxvertexcount()] - 2 in the geometry shader.
        const int CIRCLE_GEOMETRY_MAX_SIDES = 32;
        const int CIRCLE_MIN_SIDES = 3;

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
        Color[] _singleColorList;

        public SpriteBatch()
        {
            _singleColorList = new Color[1];
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

        protected SpriteCluster ConfigureNewCluster(SpriteClipZone clip, ITexture2D texture, IMaterial material, ClusterFormat format, int needed)
        {
            if (_clusterBank.Length == _clusterCount)
            {
                Array.Resize(ref _clusterBank, _clusterBank.Length + CLUSTER_EXPANSION);

                // Initialize new cluster slots.
                for (int i = _clusterCount; i < _clusterBank.Length; i++)
                    _clusterBank[i] = new SpriteCluster(SPRITE_EXPANSION + needed);
            }

            SpriteCluster cluster = _clusterBank[_clusterCount];
            cluster.Format = format;
            cluster.Texture = texture;
            cluster.Material = material;
            cluster.drawnTo = 0;
            cluster.drawnFrom = 0;
            cluster.SpriteCount = 0;

            if (cluster.Sprites.Length < needed)
                Array.Resize(ref cluster.Sprites, needed + SPRITE_EXPANSION);

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



        /// <summary>Adds a sprite to the batch.</summary>
        /// <param name="texture"></param>
        /// <param name="position"></param>
        /// <param name="source"></param>
        /// <param name="color"></param>
        /// <param name="rotation">Rotation, in radians.</param>
        /// <param name="origin"></param>
        public void Draw(ITexture2D texture, Vector2F position, Rectangle source, Color color, float rotation, Vector2F scale, Vector2F origin, IMaterial material = null)
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
        public void Draw(ITexture2D texture, Rectangle source, Rectangle destination, Color color, float rotation, Vector2F origin, IMaterial material)
        {
            int spriteID = 0;
            SpriteCluster cluster = GetCluster(_clipZones[_curClip], texture, material, ClusterFormat.Sprite, 1, out spriteID);

            // Set the sprite info
            cluster.Sprites[spriteID] = new SpriteVertex()
            {
                Position = new Vector2F(destination.Left, destination.Top),
                Size = new Vector2F(destination.Width, destination.Height),
                UV = new Vector4F(source.X, source.Y, source.Right, source.Bottom),
                Color = color,
                Origin = origin,
                Rotation = rotation,
            };

            cluster.SpriteCount++;
        }

        /// <summary>
        /// Adds the contents of the specified <see cref="SpriteBatchCache"/> to the current <see cref="SpriteBatch"/>.
        /// </summary>
        /// <param name="cache">The cache.</param>
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
                    cluster = ConfigureNewCluster(clip, cacheCluster.Texture, cacheCluster.Material, cacheCluster.Format, cacheCluster.Sprites.Length);
                }
                else
                {
                    cluster = clip.CurCluster;
                    // See if we can merge the first cache cluster into the current. If not, create a new cluster to start copying to.
                    if (cluster.Texture != cacheCluster.Texture || cluster.Material != cacheCluster.Material || cluster.Format != cacheCluster.Format)
                        cluster = ConfigureNewCluster(clip, cacheCluster.Texture, cacheCluster.Material, cacheCluster.Format, cacheCluster.Sprites.Length);
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
                        cluster = ConfigureNewCluster(clip, cacheCluster.Texture, cacheCluster.Material, cacheCluster.Format, cacheCluster.Sprites.Length);
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
                cluster = ConfigureNewCluster(clip, texture, material, format, needed);
            }
            else
            {
                cluster = clip.CurCluster;
                if (cluster.Texture != texture || cluster.Material != material || cluster.Format != format)
                {
                    cluster = ConfigureNewCluster(clip, texture, material, format, needed);
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

        /// <summary>Gets the current clip depth. This increases and decreases with calls to <see cref="PushClip(Rectangle)"/> and <see cref="PopClip"/></summary>
        public int ClipDepth { get { return _clipCount; } }
    }
}
