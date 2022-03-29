using System.Collections.Concurrent;
using Molten.Font;
using Molten.Graphics;

namespace Molten
{
    internal class FontManager
    {
        class FontCache
        {
            public FontFile Font;

            public Dictionary<ulong, SpriteFont> Instances = new Dictionary<ulong, SpriteFont>();
        }

        Engine _engine;
        ConcurrentDictionary<string, FontCache> _cache;

        internal FontManager(Engine engine)
        {
            _engine = engine;
            _cache = new ConcurrentDictionary<string, FontCache>();
        }

        /// <summary>
        /// Attempts to load a <see cref="SpriteFont"/>. If a font with the same parameters already exists, it's existing instance will be returned.
        /// </summary>
        /// <param name="log">The log with which to log messages or errors while loading the font.</param>
        /// <param name="path">The file path or system font name.</param>
        /// <param name="ptSize">the font size.</param>
        /// <param name="tabSize">The number of space characters which represent one tab-space.</param>
        /// <param name="texturePageSize">The size of the sprite font's internal texture pages.</param>
        /// <param name="pointsPerCurve">The maximum number of points within a font character curvature. This is akin to a level of detail or resolution.</param>
        /// <param name="initialPages">The number of initial pages inside the font upon creation, if it doesn't already exist.</param>
        /// <param name="charPadding">The spacing between characters.</param>
        /// <returns></returns>
        internal SpriteFont GetFont(Logger log, string path, 
            int ptSize,
            int tabSize = 3,
            int texturePageSize = 512,
            int pointsPerCurve = 16,
            int initialPages = 1,
            int charPadding = 2)
        {
            if(ptSize > byte.MaxValue)
            {
                log.Error($"Font size cannot be greater than {byte.MaxValue}");
                return null;
            }

            if (tabSize > byte.MaxValue)
            {
                log.Error($"Tab size cannot be greater than {byte.MaxValue}");
                return null;
            }

            if (texturePageSize > ushort.MaxValue)
            {
                log.Error($"Texture page size cannot be greater than {ushort.MaxValue}");
                return null;
            }

            if (pointsPerCurve > ushort.MaxValue)
            {
                log.Error($"The number of points per curve cannot be greater than {ushort.MaxValue}");
                return null;
            }

            if (charPadding > byte.MaxValue)
            {
                log.Error($"Character padding cannot be greater than {byte.MaxValue}");
                return null;
            }

            path = path.ToLower();

            ulong hash = (ulong)ptSize << 56;       // [ptSize - 1 byte/8-bit]
            hash |= (ulong)tabSize << 48;           // [tabSize - 1 byte/8-bit]
            hash |= (ulong)texturePageSize << 32;   // [texturePageSize - 2 bytes/16-bit]
            hash |= (ulong)pointsPerCurve << 16;    // [pointsPerCurve - 2 bytes/16-bit]
            hash |= (ulong)charPadding << 8;        // [charPadding - 1 byte/8-bit]
            hash |= 0;                              // [RESERVERD - 1 byte/8-bit]

            FontFile fFile = null;
            if (_cache.TryGetValue(path, out FontCache cache))
            {
                fFile = cache.Font;
                if (cache.Instances.TryGetValue(hash, out SpriteFont font))
                    return font;
            }

            if (fFile == null)
            {
                FileInfo fInfo = new FileInfo(path);

                if (fInfo.Exists)
                {
                    using (Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                    {
                        using (FontReader reader = new FontReader(stream, log, path))
                            fFile = reader.ReadFont(true);
                    }
                }
                else
                {
                    string sysFontName = fInfo.Name;

                    if (!string.IsNullOrEmpty(fInfo.Extension))
                        sysFontName = sysFontName.Replace(fInfo.Extension, "");

                    using (FontReader reader = new FontReader(sysFontName, log))
                        fFile = reader.ReadFont(true);
                }
            }

            if (fFile.HasFlag(FontFlags.Invalid))
            {
                log.Error($"The font '{path}' is invalid. Unable to create sprite font");
                return null;
            }

            // Create a new instance of the font
            SpriteFont newFont = new SpriteFont(_engine.Renderer, fFile, ptSize,
                tabSize, texturePageSize, pointsPerCurve, initialPages, charPadding);

            if(cache == null)
            {
                cache = new FontCache();
                cache.Font = fFile;

                if (!_cache.TryAdd(path, cache))
                    cache = _cache[path];
            }

            cache.Instances.TryAdd(hash, newFont);
            newFont.OnDisposing += NewFont_OnDisposing;

            return newFont;
        }

        private void NewFont_OnDisposing(EngineObject o)
        {
            SpriteFont sf = o as SpriteFont;

            foreach (FontCache cache in _cache.Values)
            {
                if (cache.Font == sf.Font)
                    cache.Instances.Remove(sf.FontHash);
            }
        }
    }
}
