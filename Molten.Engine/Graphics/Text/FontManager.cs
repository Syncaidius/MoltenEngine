using System.Collections.Concurrent;
using Molten.Font;
using Molten.Graphics;

namespace Molten.Graphics
{
    internal class FontManager
    {
        Engine _engine;
        object _locker;

        ConcurrentDictionary<string, FontFile> _fileByPath;
        ConcurrentDictionary<FontFile, TextFontSource> _cache;

        internal FontManager(Engine engine)
        {
            _locker = new object();
            _engine = engine;
            _cache = new ConcurrentDictionary<FontFile, TextFontSource>();
            _fileByPath = new ConcurrentDictionary<string, FontFile>();
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
        internal TextFontSource GetFont(Logger log, string path, 
            int texturePageSize = 512,
            int initialPages = 1,
            int charPadding = 2)
        {

            if (texturePageSize > ushort.MaxValue)
            {
                log.Error($"Texture page size cannot be greater than {ushort.MaxValue}");
                return null;
            }

            if (charPadding > byte.MaxValue)
            {
                log.Error($"Character padding cannot be greater than {byte.MaxValue}");
                return null;
            }

            path = path.ToLower();

            lock (_locker)
            {
                if (_fileByPath.TryGetValue(path, out FontFile fFile))
                    return _cache[fFile];

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

                if (fFile.HasFlag(FontFlags.Invalid))
                {
                    log.Error($"The font '{path}' is invalid. Unable to create sprite font");
                    return null;
                }

                // Create a new instance of the font
                TextFontSource newFont = new TextFontSource(_engine.Renderer, fFile, texturePageSize, initialPages, charPadding);

                _fileByPath.TryAdd(path, fFile);
                _cache.TryAdd(fFile, newFont);
                newFont.OnDisposing += NewFont_OnDisposing;

                return newFont;
            }
        }

        private void NewFont_OnDisposing(EngineObject o)
        {
            TextFontSource tfs = o as TextFontSource;

            string pathToRemove = null;
            lock (_locker)
            {
                foreach (string key in _fileByPath.Keys)
                {
                    if (_fileByPath[key] == tfs.Font)
                    {
                        pathToRemove = key;
                        _cache.TryRemove(tfs.Font, out tfs);
                        break;
                    }
                }

                _fileByPath.TryRemove(pathToRemove, out FontFile fFile);
            }
        }
    }
}
